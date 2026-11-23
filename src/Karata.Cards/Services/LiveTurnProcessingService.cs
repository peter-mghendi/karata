using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Karata.Cards.Support.Exceptions;
using Karata.Kit.Cards.Engine;
using Karata.Kit.Cards.Models;
using Microsoft.AspNetCore.SignalR;
using static Karata.Kit.Cards.Models.CardRequestLevel;
using static Karata.Kit.Cards.Models.GameStatus;
using static Karata.Kit.Cards.Models.HandStatus;
using static Karata.Kit.Cards.Models.TurnType;
using static Karata.Pips.Card.CardFace;

namespace Karata.Cards.Services;

/// <summary>
/// Orchestrates live turn processing: validation, delta generation, state mutation, notifications, and persistence.
/// </summary>
public sealed class LiveTurnProcessingService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILogger<LiveTurnProcessingService> logger,
    CardsContext context,
    IKarataEngine engine,
    Guid roomId,
    string playerId,
    string connection
) : LiveRoomAwareService(players, spectators, roomId, playerId)
{
    public async Task ExecuteAsync(List<Card> cards)
    {
        var player = (await context.Users.FindAsync(CallerPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;

        EnsureCanPlay(room.Game, cards);

        await (engine.EvaluateTurn(game: room.Game, cards: [..cards]) switch
        {
            TurnDelta delta => AcceptAsync(room, player, cards, delta),
            TurnProblem problem => RejectAsync(room, cards, problem)
        });
    }

    private void EnsureCanPlay(Game game, List<Card> cards)
    {
        switch (game)
        {
            case { Status: Lobby }: throw new GameNotStartedException();
            case { Status: Over }: throw new GameOverException();
            case { Hands: var hands } when hands.Count(hand => hand.Status is Active) < 2:
                throw new NotEnoughPlayersException();
            case { CurrentHand.Player.Id: var current } when current != CallerPlayerId:
                throw new InvalidTurnException();
        }

        if (cards.Distinct().Count() != cards.Count || !game.CurrentHand.Cards.IsSupersetOf(cards))
            throw new SuspiciousCardsException();
    }

    private async Task RejectAsync(Room room, List<Card> cards, TurnProblem problem)
    {
        logger.LogDebug(
            "Invalid turn. Game: {Game}, Problem: {Problem}, Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}.",
            room.Id,
            problem.Kind,
            room.Game.Pile.Count,
            room.Game.Deck.Count,
            room.Game.Pick
        );

        room.Game.CurrentHand.Turns.Add(new Turn
        {
            Type = Fail,
            CardsPlayed = [..cards],
            Problem = problem,
            Hand = room.Game.CurrentHand,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
        await Caller.TurnRejected(RoomId, problem);
    }

    private async Task AcceptAsync(Room room, User player, List<Card> cards, TurnDelta delta)
    {
        var game = room.Game;
        var turn = new Turn
        {
            Type = Play,
            CardsPlayed = [..cards],
            Delta = delta,
            Hand = game.CurrentHand,
            CreatedAt = DateTimeOffset.UtcNow
        };

        turn.Request = game.Request = await DetermineCardRequest(game, delta);
        game.CurrentHand.Turns.Add(turn);
        await Caller.TurnAcknowledged(RoomId);

        if (!TryApply(game, turn, out var dealt))
        {
            await EndAsync(room, turn, GameResultData.DeckExhaustion());
            return;
        }

        await UpdateTableState(game, turn, dealt);
        if (await CheckWinAsync(game, player, turn))
        {
            await EndAsync(room, turn, GameResultData.Win(player));
            return;
        }

        game.Pick = delta.Give;
        game.AdvanceTurn();
        turn.GameSnapshot = game;

        await context.SaveChangesAsync();
        await BroadcastTurnCommitted(game);
    }

    private async Task<Card?> DetermineCardRequest(Game game, TurnDelta delta)
    {
        var request = game.RequestLevel switch
        {
            CardRequest when delta.RemoveRequestLevels is 1 => None.Of(game.Request!.Suit),
            CardRequest when delta.RemoveRequestLevels >= 2 => null,
            SuitRequest when delta.RemoveRequestLevels >= 1 => null,
            _ => game.Request
        };

        if (delta.RequestLevel is NoRequest)
        {
            logger.LogDebug("Kept request {Request} in room {Room}.", request, RoomId);
            return request;
        }

        var specific = delta.RequestLevel is CardRequest;
        var requested = await PlayerConnection(connection).PromptCardRequest(RoomId, specific);

        // The answer comes from the client: a suit request can't be turned into a specific card request.
        if (!specific && requested is { Face: not None }) requested = None.Of(requested.Suit);

        logger.LogDebug("Requested {Request} for level {Level} in room {Room}.", requested, delta.RequestLevel, RoomId);
        return requested;
    }

    /// <summary>
    /// Moves the played cards to the pile, and deals the player any cards they have to pick.
    /// </summary>
    /// <returns>False if there aren't enough cards left to deal, which ends the game.</returns>
    private bool TryApply(Game game, Turn turn, out List<Card> dealt)
    {
        var delta = turn.Delta!;

        game.CurrentHand.Cards.RemoveWhere(delta.Cards.Contains);
        delta.Cards.ForEach(game.Pile.Push);
        game.IsReversed ^= delta.Reverse;

        dealt = [];
        if (delta.Pick is 0) return true;

        if (!game.Deck.TryDealMany(delta.Pick, out dealt))
        {
            // Everything but the top card of the pile can be shuffled back into the deck.
            if (delta.Pick > game.Pile.Count + game.Deck.Count - 1)
            {
                dealt = [];
                turn.DeckExhausted = true;
                return false;
            }

            turn.ReclaimedPile = true;
            game.Pile.Reclaim().ToList().ForEach(game.Deck.Push);

            game.Deck.ShuffleInPlace();
            dealt = game.Deck.DealMany(delta.Pick);
        }

        turn.CardsPicked = dealt;
        foreach (var card in dealt) game.CurrentHand.Cards.Add(card);

        return true;
    }

    private async Task UpdateTableState(Game game, Turn turn, List<Card> dealt)
    {
        var others = game.HandsExceptPlayerId(CallerPlayerId);
        var handId = game.CurrentHand.Id;

        await Caller.MoveCardsFromHandToPile(RoomId, handId, turn.Delta!.Cards, true);
        await Hands(others).MoveCardsFromHandToPile(RoomId, handId, turn.Delta!.Cards, false);
        await RoomSpectators.MoveCardsFromHandToPile(RoomId, handId, turn.Delta!.Cards, false);

        if (turn.ReclaimedPile)
        {
            await RoomPlayers.ReclaimPile(RoomId);
            await RoomSpectators.ReclaimPile(RoomId);
        }

        await Caller.MoveCardsFromDeckToHand(RoomId, handId, dealt);

        var dummies = Enumerable.Repeat(new Card(), dealt.Count).ToList();
        await Hands(others).MoveCardsFromDeckToHand(RoomId, handId, dummies);
        await RoomSpectators.MoveCardsFromDeckToHand(RoomId, handId, dummies);
    }

    /// <returns>True if this turn wins the game.</returns>
    private async Task<bool> CheckWinAsync(Game game, User player, Turn turn)
    {
        var hand = game.CurrentHand;

        if (hand.Cards.Count > 0)
        {
            try
            {
                turn.IsLastCard = await PlayerConnection(connection).PromptLastCardRequest(RoomId);
                logger.LogDebug("User {User} last card status is {Status} in room {Room}.", player.Id, turn.IsLastCard, RoomId);
            }
            catch (Exception exception)
            {
                turn.IsLastCard = false;
                logger.LogError(exception, "Failed to request last card status for {User} in room {Room}.", player.Id, RoomId);
            }

            hand.IsLastCard = turn.IsLastCard;
            return false;
        }

        if (hand.IsLastCard && !turn.Delta!.Cards.Last().IsSpecial) return true;

        turn.IsCardless = true;
        return false;
    }

    private async Task EndAsync(Room room, Turn turn, GameResultData data)
    {
        logger.LogDebug(
            "Game ending. Game: {Game}, Reason: {Reason}, Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}.",
            room.Id,
            data.Reason,
            room.Game.Pile.Count,
            room.Game.Deck.Count,
            room.Game.Pick
        );

        var result = new GameResult
        {
            Reason = data.Reason,
            ReasonType = data.ReasonType,
            ResultType = data.ResultType,
            CompletedAt = data.CompletedAt,
            Winner = data.Winner is null ? null : await context.Users.FindAsync(data.Winner.Id)
        };

        (room.Game.Status, room.Game.Result) = (Over, result);
        turn.GameResult = result;
        turn.GameSnapshot = room.Game;

        await context.SaveChangesAsync();

        await RoomPlayers.UpdateGameStatus(RoomId, room.Game);
        await RoomSpectators.UpdateGameStatus(RoomId, room.Game);
        await RoomPlayers.EndGame(RoomId, data);
        await RoomSpectators.EndGame(RoomId, data);
    }

    private async Task BroadcastTurnCommitted(Game game)
    {
        foreach (var data in from hand in game.Hands select (Hand: hand, Game: Enrich.ForHand(game, hand)))
            await Hand(data.Hand).TurnCommitted(RoomId, data.Game);
        await RoomSpectators.TurnCommitted(RoomId, game);
    }
}