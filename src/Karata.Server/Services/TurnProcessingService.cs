using System.Text.Json;
using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Karata.Server.Support.Exceptions;
using Microsoft.AspNetCore.SignalR;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.CardRequestLevel;
using static Karata.Shared.Models.GameStatus;

namespace Karata.Server.Services;

public class TurnProcessingService(
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    KarataEngineFactory factory,
    ILogger<TurnProcessingService> logger,
    User player,
    Room room,
    string client
) : HubAwareService(hub, room, player, client)
{
    public async Task ExecuteAsync(List<Card> cards)
    {
        ValidateGameState();
        var engine = factory.Create(Game, cards.ToList());
        
        (Game.Pick, Game.Give) = (Game.Give, 0);
        engine.EnsureTurnIsValid();
        
        var delta = engine.GenerateTurnDelta();
        var turn = new Turn { Cards = cards.ToList(), Delta = delta };
        logger.LogDebug("User {User} performed turn {Turn}.", CurrentPlayer, JsonSerializer.Serialize(turn));

        await DetermineCardRequest(turn);
        ApplyTurnDelta(turn);

        await NotifyClientsOfGameState();
        await EnsurePendingCardsPicked();
        await CheckRemainingCards();

        Game.CurrentTurn = Game.NextTurn;

        await Everyone.UpdateTurn(Game.CurrentTurn);
        await context.SaveChangesAsync();
    }
    
    private void ValidateGameState()
    {
        // Check game status
        if (Game.Status is not Ongoing)
        {
            throw new GameHasNotStartedException();
        }

        // Check turn
        if (Game.CurrentHand.Player.Id != CurrentPlayer.Id)
        {
            throw new NotYourTurnException();
        }
    }
    
    private async Task NotifyClientsOfGameState()
    {
        var turn = Game.CurrentHand.Turns.Last();

        await Me.NotifyTurnProcessed();
        await Me.RemoveCardRangeFromHand(turn.Delta.Cards);
        await Others.RemoveCardsFromPlayerHand(Game.CurrentHand.Player.ToData(), turn.Delta.Cards.Count);
        await Everyone.AddCardRangeToPile(turn.Delta.Cards);
    }

    private async Task DetermineLastCardStatus(Turn turn)
    {
        turn.IsLastCard = await Prompt.PromptLastCardRequest();
        if (turn.IsLastCard)
        {
            Game.CurrentHand.IsLastCard = true;
            await Others.ReceiveSystemMessage(Messages.LastCard(Game.CurrentHand.Player));
        }
    }

    private async Task DetermineCardRequest(Turn turn)
    {
        Game.Request = Game.RequestLevel switch
        {
            CardRequest when turn.Delta.RemoveRequestLevels is 1 => None.Of(Game.Request!.Suit), // One cancels the card request, leaves the suit
            CardRequest when turn.Delta.RemoveRequestLevels >= 2 => null, // Anything above 1 cancels a suit request
            SuitRequest when turn.Delta.RemoveRequestLevels >= 1 => null, // Anything above 0 cancels a card request
            _ => Game.Request
        };

        // TODO: Do I need this broadcast or just send final state - players will not be able to act on this intermediate state anyway?
        // Need to make sure it's handled below.
        await Everyone.SetCurrentRequest(Game.Request);

        var level = turn.Delta.RequestLevel;
        if (level is NoRequest)
        {
            logger.LogDebug("No card request for level {RequestLevel} in room {Room}.", level, Room.Id);
            return;
        }

        turn.Request = await Prompt.PromptCardRequest(specific: level is CardRequest);
        logger.LogDebug("Requested {Request} for level {RequestLevel} in room {Room}.", turn.Request, level, Room.Id);
        
        Game.Request = turn.Request;
        await Everyone.SetCurrentRequest(Game.Request);
    }

    private void ApplyTurnDelta(Turn turn)
    {
        Game.CurrentHand.Turns.Add(turn);
        Game.CurrentHand.Cards.RemoveAll(turn.Delta.Cards.Contains);
        foreach (var card in turn.Delta.Cards) Game.Pile.Push(card);

        if (turn.Delta.Reverse) Game.IsReversed = !Game.IsReversed;
        (Game.Give, Game.Pick) = (turn.Delta.Give, turn.Delta.Pick);
    }

    private async Task EndGame(GameResult result)
    {
        logger.LogDebug(
            "Game Ending - {Game}: {Reason}. Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}.",
            Room.Id,
            result.Reason,
            Game.Pile.Count,
            Game.Deck.Count,
            Game.Pick
        );

        Game.Status = Over;
        Game.Result = result;
        
        await context.SaveChangesAsync();
        
        await Me.NotifyTurnProcessed();
        await Everyone.ReceiveSystemMessage(Messages.GameOver(result));
        await Everyone.UpdateGameStatus(Over);
        await Everyone.EndGame();
    }

    private async Task EnsurePendingCardsPicked()
    {
        // Check whether there are cards to pick.
        if (Game.Pick > 0)
        {
            // Remove card from pile
            if (!Game.Deck.TryDealMany(Game.Pick, out var dealt))
            {
                if (Game.Pile.Count + Game.Deck.Count - 1 > Game.Pick)
                {
                    // Reclaim pile
                    foreach (var card in Game.Pile.Reclaim()) Game.Deck.Push(card);
                    await Everyone.ReclaimPile();

                    // Shuffle & deal
                    Game.Deck.Shuffle();
                    dealt = Game.Deck.DealMany(Game.Pick);
                }
                else
                {
                    await EndGame(GameResult.DeckExhaustion());
                    return;
                }
            }

            await Everyone.RemoveCardsFromDeck(1);

            // Add cards to player hand and reset counter
            Game.CurrentHand.Cards.AddRange(dealt);
            await Me.AddCardRangeToHand(dealt);
            await Others.AddCardsToPlayerHand(Game.CurrentHand.Player.ToData(), dealt.Count);
            Game.Pick = 0;
        }
    }

    private async Task CheckRemainingCards()
    {
        var turn = Game.CurrentHand.Turns.Last();
        var player = Game.CurrentHand.Player;

        // Check whether the game is over.
        if (Game.CurrentHand.Cards.Count == 0)
        {
            if (Game.CurrentHand.IsLastCard && !turn.Delta.Cards[^1].IsSpecial())
            {
                await EndGame(GameResult.Win(winner: player));
                return;
            }

            await Others.ReceiveSystemMessage(Messages.Cardless(player));
        }
        else
        {
            await DetermineLastCardStatus(turn);
        }
    }
}