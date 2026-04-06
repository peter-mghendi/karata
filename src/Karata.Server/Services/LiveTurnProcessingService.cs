using System.Text.Json;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Karata.Kit.Engine.Exceptions;
using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support.Exceptions;
using Microsoft.AspNetCore.SignalR;
using static Karata.Cards.Card.CardFace;
using static Karata.Kit.Domain.Models.CardRequestLevel;
using static Karata.Kit.Domain.Models.GameStatus;
using static Karata.Kit.Domain.Models.HandStatus;
using static Karata.Kit.Domain.Models.TurnType;

namespace Karata.Server.Services;

/// <summary>
/// Orchestrates live turn processing: validation, delta generation, state mutation, notifications, and persistence.
/// </summary>
public sealed class LiveTurnProcessingService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILogger<LiveTurnProcessingService> logger,
    KarataContext context,
    IKarataEngine engine,
    Guid roomId,
    string playerId,
    string connection
) : LiveRoomAwareService(players, spectators, roomId, playerId)
{
    private readonly EngineData _details = new()
    {
        Name = engine.Name,
        Date = ThisAssembly.Git.CommitDate,
        Branch = ThisAssembly.Git.Branch,
        Version = ThisAssembly.Git.Sha,
        Revision = ThisAssembly.Git.Commit,
    };

    public async Task ExecuteAsync(List<Card> cards)
    {
        var player = (await context.Users.FindAsync(CallerPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;

        EnsureTurnIsValid(room, cards);
        var turn = new Turn
        {
            CardsPlayed = [..cards],
            Type = Play,
            Hand = room.Game.CurrentHand,
            CreatedAt = DateTimeOffset.UtcNow,
            Metadata = new TurnMetadata { Engine = _details }
        };

        try
        {
            // This is a new turn. Current player picks cards given by previous player. Reset cards given.
            (room.Game.Pick, room.Game.Give) = (room.Game.Give, 0);

            turn.Delta = engine.EvaluateTurn(game: room.Game, cards: [..cards]);
            turn.Request = room.Game.Request = await DetermineCardRequest(room, turn);

            logger.LogDebug("User {User} performed turn {Turn}.", CallerPlayerId, JsonSerializer.Serialize(turn));
        }
        catch (TurnValidationException exception)
        {
            logger.LogDebug(
                "Invalid turn. Game: {Game}, Problem: {Reason}, Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}.",
                room.Id,
                exception.Problem,
                room.Game.Pile.Count,
                room.Game.Deck.Count,
                room.Game.Pick
            );

            turn.Type = Fail;
            turn.Metadata.Problem = exception.Problem;
            room.Game.CurrentHand.Turns.Add(turn);

            await context.SaveChangesAsync();
            throw;
        }

        room.Game.CurrentHand.Turns.Add(turn);
        await Me.TurnAccepted();

        try
        {
            // Updates only the table, to allow players to make decisions on last card status.
            EnsureTurnApplied(room, turn, out var dealt);
            await UpdateTableState(room, turn, dealt);
            await CheckWinConditions(room, player, turn);
        }
        catch (EndGameException exception)
        {
            logger.LogDebug(
                "Game Ending. Game: {Game}, Reason: {Reason}, Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}.",
                room.Id,
                exception.Result.Reason,
                room.Game.Pile.Count,
                room.Game.Deck.Count,
                room.Game.Pick
            );

            var result = new GameResult
            {
                Reason = exception.Result.Reason,
                ReasonType = exception.Result.ReasonType,
                ResultType = exception.Result.ResultType,
                CompletedAt = exception.Result.CompletedAt,
                Winner = exception.Result.Winner is null
                    ? null
                    : await context.Users.FindAsync(exception.Result.Winner!.Id)
            };

            (room.Game.Status, room.Game.Result) = (Over, result);
            turn.GameSnapshot = room.Game;

            if (exception.Result.ResultType is GameResultType.Win) context.Activities.Add(Activity.GameWon(room));

            await context.SaveChangesAsync();

            await Me.TurnAccepted();
            await RoomPlayers.UpdateGameStatus(room.Game.Status);
            await RoomSpectators.UpdateGameStatus(room.Game.Status);
            await RoomPlayers.EndGame(exception.Result);
            await RoomSpectators.EndGame(exception.Result);
        }

        turn.GameSnapshot = room.Game;
        room.Game.AdvanceTurn();

        await context.SaveChangesAsync();
        await BroadcastTurn(room, player, turn);
    }

    private void EnsureTurnIsValid(Room room, List<Card> cards)
    {
        if (room.Game.Status is Lobby) throw new GameNotStartedException();
        if (room.Game.Status is Over) throw new GameOverException();
        if (room.Game.Hands.Count(hand => hand.Status is Online or Offline) < 2) throw new NotEnoughPlayersException();
        if (room.Game.CurrentHand.Player.Id != CallerPlayerId) throw new InvalidTurnException();
        if (!room.Game.CurrentHand.Cards.ToHashSet().IsSupersetOf(cards) || cards.Distinct().Count() != cards.Count)
            throw new SuspiciousCardsException();
    }

    private async Task<Card?> DetermineCardRequest(Room room, Turn turn)
    {
        var request = room.Game.RequestLevel switch
        {
            CardRequest when turn.Delta!.RemoveRequestLevels is 1 => None.Of(room.Game.Request!.Suit),
            CardRequest when turn.Delta!.RemoveRequestLevels >= 2 => null,
            SuitRequest when turn.Delta!.RemoveRequestLevels >= 1 => null,
            _ => room.Game.Request
        };

        var level = turn.Delta!.RequestLevel;
        if (level is NoRequest)
        {
            logger.LogDebug("No card request for level {RequestLevel} in room {Room}.", level, room.Id);
            return request;
        }

        request = await PlayerConnection(connection).PromptCardRequest(specific: level is CardRequest);
        logger.LogDebug("Requested {Request} for level {RequestLevel} in room {Room}.", request, level, room.Id);

        return request;
    }

    private static void EnsureTurnApplied(Room room, Turn turn, out List<Card> dealt)
    {
        room.Game.CurrentHand.Cards.RemoveAll(turn.Delta!.Cards.Contains);
        turn.Delta!.Cards.ForEach(room.Game.Pile.Push);

        room.Game.IsReversed ^= turn.Delta!.Reverse;
        (room.Game.Give, room.Game.Pick) = (turn.Delta!.Give, turn.Delta!.Pick);

        dealt = [];
        if (room.Game.Pick <= 0) return;

        if (!room.Game.Deck.TryDealMany(room.Game.Pick, out dealt))
        {
            if (room.Game.Pick > room.Game.Pile.Count + room.Game.Deck.Count - 1)
            {
                turn.DeckExhausted = true;
                throw EndGameException.DeckExhaustion();
            }

            turn.ReclaimedPile = true;
            room.Game.Pile.Reclaim().ToList().ForEach(room.Game.Deck.Push);

            room.Game.Deck.ShuffleInPlace();
            dealt = room.Game.Deck.DealMany(room.Game.Pick);
        }

        room.Game.Pick = 0;
        room.Game.CurrentHand.Cards.AddRange(dealt);
        turn.CardsPicked = dealt;
    }

    private async Task UpdateTableState(Room room, Turn turn, List<Card> dealt)
    {
        await Me.MoveCardsFromHandToPile(room.Game.CurrentHand.Id, turn.Delta!.Cards, true);
        await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId))
            .MoveCardsFromHandToPile(room.Game.CurrentHand.Id, turn.Delta!.Cards, false);
        await RoomSpectators.MoveCardsFromHandToPile(room.Game.CurrentHand.Id, turn.Delta!.Cards, false);

        if (turn.ReclaimedPile)
        {
            await RoomPlayers.ReclaimPile();
            await RoomSpectators.ReclaimPile();
        }

        await Me.MoveCardsFromDeckToHand(room.Game.CurrentHand.Id, dealt);

        var dummies = Enumerable.Repeat(new Card(), dealt.Count).ToList();
        await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId))
            .MoveCardsFromDeckToHand(room.Game.CurrentHand.Id, dummies);
        await RoomSpectators.MoveCardsFromDeckToHand(room.Game.CurrentHand.Id, dummies);
    }

    private async Task CheckWinConditions(Room room, User player, Turn turn)
    {
        if (room.Game.CurrentHand.Cards.Count > 0)
        {
            try
            {
                turn.IsLastCard = await PlayerConnection(connection).PromptLastCardRequest();
            }
            catch (Exception)
            {
                turn.IsLastCard = false;
            }
            finally
            {
                room.Game.CurrentHand.IsLastCard = turn.IsLastCard;
            }

            return;
        }

        if (room.Game.CurrentHand.IsLastCard && !turn.Delta!.Cards.Last().IsSpecial)
            throw EndGameException.Win(winner: player);

        turn.IsCardless = true;
    }
    
    private async Task BroadcastTurn(Room room, User player, Turn turn)
    {
        var resolution = new TurnResolution(
            room.Game.CurrentTurn,
            player,
            room.Game.Request,
            room.Game.Give,
            turn.IsCardless,
            turn.IsLastCard
        );

        await RoomPlayers.TurnCommitted(resolution);
        await RoomSpectators.TurnCommitted(resolution);
    }
}