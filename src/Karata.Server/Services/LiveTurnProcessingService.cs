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

        _ = EnsureTurnIsValid(room, cards);
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
        await Me.TurnAcknowledged(RoomId);

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

            await Me.TurnAcknowledged(RoomId);
            await RoomPlayers.UpdateGameStatus(RoomId, room.Game);
            await RoomSpectators.UpdateGameStatus(RoomId, room.Game);
            await RoomPlayers.EndGame(RoomId, exception.Result);
            await RoomSpectators.EndGame(RoomId, exception.Result);
        }

        turn.GameSnapshot = room.Game;
        room.Game.AdvanceTurn();

        await context.SaveChangesAsync();
        await BroadcastTurnCommitted(room.Game);
    }

    private bool EnsureTurnIsValid(Room room, List<Card> cards) => room.Game switch
    {
        { Status: Lobby } => throw new GameNotStartedException(),
        { Status: Over } => throw new GameOverException(),
        { Hands: var hands } when hands.Count(hand => hand.Status is Online or Offline) < 2 =>
            throw new NotEnoughPlayersException(),
        { CurrentHand.Player: var currentPlayer } when currentPlayer.Id != CallerPlayerId =>
            throw new InvalidTurnException(),
        // { CurrentHand.Cards: var held } when held.IsSupersetOf(cards) => throw new SuspiciousCardsException(),
        // not null when cards.Distinct().Count() != cards.Count => throw new SuspiciousCardsException(),
        _ => true
    };

    private async Task<Card?> DetermineCardRequest(Room room, Turn turn)
    {
        logger.LogDebug("Determining request for {Game}.", room.Id);

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
            logger.LogDebug("Kept request {Request} for level {RequestLevel} in room {Room}.", request, level, room.Id);
            return request;
        }

        request = await PlayerConnection(connection).PromptCardRequest(RoomId, specific: level is CardRequest);
        logger.LogDebug("Requested {Request} for level {RequestLevel} in room {Room}.", request, level, room.Id);

        return request;
    }

    private void EnsureTurnApplied(Room room, Turn turn, out List<Card> dealt)
    {
        logger.LogDebug("Applying turn for {Game}.", room.Id);

        room.Game.CurrentHand.Cards.RemoveWhere(turn.Delta!.Cards.Contains);
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
        turn.CardsPicked = dealt;
        foreach (var card in dealt) 
            room.Game.CurrentHand.Cards.Add(card);
    }

    private async Task UpdateTableState(Room room, Turn turn, List<Card> dealt)
    {
        logger.LogDebug("Updating table state for {Game}.", room.Id);

        await Me.MoveCardsFromHandToPile(RoomId, room.Game.CurrentHand.Id, turn.Delta!.Cards, true);
        await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId))
            .MoveCardsFromHandToPile(RoomId, room.Game.CurrentHand.Id, turn.Delta!.Cards, false);
        await RoomSpectators.MoveCardsFromHandToPile(RoomId, room.Game.CurrentHand.Id, turn.Delta!.Cards, false);

        if (turn.ReclaimedPile)
        {
            await RoomPlayers.ReclaimPile(RoomId);
            await RoomSpectators.ReclaimPile(RoomId);
        }

        await Me.MoveCardsFromDeckToHand(RoomId, room.Game.CurrentHand.Id, dealt);

        var dummies = Enumerable.Repeat(new Card(), dealt.Count).ToList();
        await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId))
            .MoveCardsFromDeckToHand(RoomId, room.Game.CurrentHand.Id, dummies);
        await RoomSpectators.MoveCardsFromDeckToHand(RoomId, room.Game.CurrentHand.Id, dummies);
    }

    private async Task CheckWinConditions(Room room, User player, Turn turn)
    {
        logger.LogDebug("Checking win for player {Player} in {Game}.", player, room.Id);

        if (room.Game.CurrentHand.Cards.Count > 0)
        {
            logger.LogDebug("Requesting last card status for player {Player} in {Game}.", player, room.Id);

            try
            {
                turn.IsLastCard = await PlayerConnection(connection).PromptLastCardRequest(RoomId);
                logger.LogInformation("User {User} last card status is {Status} in Game {Game}", player.Id,
                    turn.IsLastCard, room.Game.CurrentHand);
            }
            catch (Exception ex)
            {
                turn.IsLastCard = false;
                logger.LogError(ex, "Failed to request last card status for {User} in {Game}", player.Id, room.Id);
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

    private async Task BroadcastTurnCommitted(Game game)
    {
        foreach (var data in from hand in game.Hands select (Hand: hand, Game: Enrich.ForUser(game, hand)))
            await Hand(data.Hand).TurnCommitted(RoomId, data.Game);
        await RoomSpectators.TurnCommitted(RoomId, game);
    }
}