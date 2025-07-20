using System.Text.Json;
using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Engine.Exceptions;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.CardRequestLevel;
using static Karata.Server.Models.TurnType;
using static Karata.Shared.Models.GameStatus;

namespace Karata.Server.Services;

/// <summary>
/// Orchestrates turn processing: validation, delta generation, state mutation, notifications, and persistence.
/// </summary>
public class TurnProcessingService(
    IHubContext<PlayerHub, IPlayerClient> hub,
    ILogger<TurnProcessingService> logger,
    KarataContext context,
    TurnManager turns,
    UserManager<User> users,
    Guid room,
    string player,
    string connection
) : HubAwareService(hub, room, player)
{
    public async Task ExecuteAsync(List<Card> cards)
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var engine = new KarataEngine { Game = room.Game, Cards = [..cards] };
        
        try
        {
            ValidateGameState(room);
            (room.Game.Pick, room.Game.Give) = (room.Game.Give, 0);

            engine.EnsureTurnIsValid();
            var delta = engine.GenerateTurnDelta();
            var turn = new Turn
            {
                Cards = [..cards],
                Delta = delta,
                Type = Play,
                Hand = room.Game.CurrentHand,
                CreatedAt = DateTimeOffset.UtcNow
            };
            logger.LogDebug("User {User} performed turn {Turn}.", CurrentPlayerId, JsonSerializer.Serialize(turn));

            await DetermineCardRequest(room, turn);
            ApplyTurnDelta(room, turn);

            await NotifyClientsOfGameState(player, turn);
            await EnsurePendingCardsPicked(room, turn);
            await CheckRemainingCards(room, player, turn);
            
            turns.Advance(room.Game);

            await Room.UpdatePick(room.Game.Give);
            await Room.UpdateTurn(room.Game.CurrentTurn);
            await context.SaveChangesAsync();
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
            
            (room.Game.Status, room.Game.Result) = (Over, exception.Result);
            if (exception.Result.ResultType is GameResultType.Win) context.Activities.Add(Activity.GameWon(room));
            
            await context.SaveChangesAsync();
            await Me.NotifyTurnProcessed();
            await Room.ReceiveSystemMessage(Messages.GameOver(exception.Result));
            await Room.UpdateGameStatus(Over);
            await Room.EndGame();
        }
    }
    
    private void ValidateGameState(Room room)
    {
        if (room.Game.Status is Lobby) throw new GameNotStartedException();
        if (room.Game.Status is Over) throw new GameOverException();
        if (room.Game.Hands.Count(hand => hand.Status is HandStatus.Connected) < 2) 
            throw new InsufficientPlayersException();

        if (room.Game.CurrentHand.Player.Id != CurrentPlayerId) throw new NotYourTurnException();
    }

    private async Task DetermineCardRequest(Room room, Turn turn)
    {
        room.Game.Request = room.Game.RequestLevel switch
        {
            CardRequest when turn.Delta!.RemoveRequestLevels is 1 => None.Of(room.Game.Request!.Suit), // One cancels the card request, leaves the suit
            CardRequest when turn.Delta!.RemoveRequestLevels >= 2 => null, // Anything above 1 cancels a suit request
            SuitRequest when turn.Delta!.RemoveRequestLevels >= 1 => null, // Anything above 0 cancels a card request
            _ => room.Game.Request
        };

        var level = turn.Delta!.RequestLevel;
        if (level is NoRequest)
        {
            logger.LogDebug("No card request for level {RequestLevel} in room {Room}.", level, room.Id);
            await Room.SetCurrentRequest(room.Game.Request);
            return;
        }

        turn.Request = await Client(connection).PromptCardRequest(specific: level is CardRequest);
        logger.LogDebug("Requested {Request} for level {RequestLevel} in room {Room}.", turn.Request, level, room.Id);
        
        room.Game.Request = turn.Request;
        await Room.SetCurrentRequest(room.Game.Request);
    }

    private void ApplyTurnDelta(Room room, Turn turn)
    {
        room.Game.CurrentHand.Turns.Add(turn);
        room.Game.CurrentHand.Cards.RemoveAll(turn.Delta!.Cards.Contains);
        turn.Delta!.Cards.ForEach(room.Game.Pile.Push);

        room.Game.IsReversed ^= turn.Delta!.Reverse;
        (room.Game.Give, room.Game.Pick) = (turn.Delta!.Give, turn.Delta!.Pick);
    }
    
    private async Task NotifyClientsOfGameState(User player, Turn turn)
    {
        await Me.NotifyTurnProcessed();
        await Room.MoveCardsFromHandToPile(player.ToData(), turn.Delta!.Cards);
    }

    private async Task EnsurePendingCardsPicked(Room room, Turn turn)
    {
        if (room.Game.Pick <= 0) return;

        if (!room.Game.Deck.TryDealMany(room.Game.Pick, out var dealt))
        {
            if (room.Game.Pick > room.Game.Pile.Count + room.Game.Deck.Count - 1)
                throw new EndGameException(GameResult.DeckExhaustion());

            // Reclaim pile
            room.Game.Pile.Reclaim().ToList().ForEach(room.Game.Deck.Push);
            await Room.ReclaimPile();

            // Shuffle & deal
            room.Game.Deck.Shuffle();
            dealt = room.Game.Deck.DealMany(room.Game.Pick);
        }

        // Add cards to player hand and reset counter
        room.Game.Pick = 0;
        room.Game.CurrentHand.Cards.AddRange(dealt);
        turn.Picked = dealt;

        await Me.MoveCardsFromDeckToHand(dealt);
        await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId))
            .MoveCardCountFromDeckToHand(room.Game.CurrentHand.Player.ToData(), dealt.Count);
    }

    private async Task CheckRemainingCards(Room room, User player, Turn turn)
    {
        if (room.Game.CurrentHand.Cards.Count != 0)
        {
            await DetermineLastCardStatus(room, turn);
            return;
        }

        if (room.Game.CurrentHand.IsLastCard && !turn.Delta!.Cards.Last().IsSpecial())
            throw new EndGameException(GameResult.Win(winner: player));

        await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).ReceiveSystemMessage(Messages.Cardless(player));
    }

    private async Task DetermineLastCardStatus(Room room, Turn turn)
    {
        turn.IsLastCard = await Client(connection).PromptLastCardRequest();
        if (turn.IsLastCard)
        {
            room.Game.CurrentHand.IsLastCard = true;
            await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).ReceiveSystemMessage(Messages.LastCard(room.Game.CurrentHand.Player));
        }
    }
}