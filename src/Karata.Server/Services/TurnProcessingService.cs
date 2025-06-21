using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class TurnProcessingService(
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    KarataEngineFactory factory,
    ILogger<TurnProcessingService> logger,
    User user,
    Room room,
    string client
) : HubAwareService(hub, room, user, client)
{
    // TODO: Clean up
    public async Task ExecuteAsync(List<Card> cards)
    {
        ValidateGameState();
        var engine = factory.Create(Game, cards);
        
        (Game.Pick, Game.Give) = (Game.Give, 0);
        engine.EnsureTurnIsValid();
        
        var delta = engine.GenerateTurnDelta();
        var turn = new Turn { Cards = cards, Delta = delta };

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
        if (Game.Status != GameStatus.Ongoing)
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
        if (turn.Delta.RemoveRequestLevels > 0)
        {
            Game.Request = null;
            await Everyone.SetCurrentRequest(null);
        }
        
        if (turn.Delta.RequestLevel is not CardRequestLevel.NoRequest)
        {
            var specific = turn.Delta.RequestLevel is CardRequestLevel.CardRequest;
            turn.Request = await Prompt.PromptCardRequest(specific);
            Game.Request = turn.Request;
            await Everyone.SetCurrentRequest(turn.Request);
        }
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

        Game.Status = GameStatus.Over;
        Game.Result = result;
        
        await context.SaveChangesAsync();
        
        await Me.NotifyTurnProcessed();
        await Everyone.ReceiveSystemMessage(Messages.GameOver(result));
        await Everyone.UpdateGameStatus(GameStatus.Over);
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