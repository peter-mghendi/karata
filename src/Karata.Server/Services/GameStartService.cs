using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Engine.Exceptions;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class GameStartService(
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    ILogger<GameStartService> logger,
    User user,
    Room room,
    string client
) : HubAwareService(hub, room, client)
{
    private const int DealCount = 4;
    
    public async Task ExecuteAsync()
    {
        ValidateGameState();
        await DealCards();
        await UpdateGameState();
        await context.SaveChangesAsync();
    }
    
    private void ValidateGameState()
    {
        var game = Room.Game;
        
        // Check caller role
        if (Room.Creator.Id != user.Id)
        {
            throw new UnauthorizedToStartException();
        }

        // Check game status
        if (game.Status == GameStatus.Ongoing)
        {
            throw new GameOngoingException();
        }

        // Check player number
        if (game.Hands.Count is < 2 or > 4)
        {
            throw new InsufficientPlayersException();
        }
    }

    private async Task DealCards()
    {
        // Shuffle deck and deal starting card
        var game = Room.Game;
        var deck = game.Deck;

        do deck.Shuffle();
        while (deck.Peek().IsSpecial());

        var top = deck.Deal();
        logger.LogDebug("Top card is {Card}.", top);

        await Everyone.RemoveCardsFromDeck(1);
        game.Pile.Push(top);
        await Everyone.AddCardRangeToPile([top]);

        logger.LogDebug("Start dealing cards to {Count} players.", game.Hands.Count);

        // Deal player cards
        // TODO: Explicit card movements (Deck -> Hand, Hand -> Pile, Pile -> Deck).
        // Needs more thought: will have to restructure PerformTurn
        foreach (var hand in game.Hands)
        {
            var dealt = deck.DealMany(DealCount);
            await Everyone.RemoveCardsFromDeck(DealCount);

            logger.LogDebug("Dealt {Count} cards to {User}.", DealCount, hand.Player.UserName);
            logger.LogDebug("Cards: {Cards}.", string.Join(", ", dealt));

            hand.Cards.AddRange(dealt);
            await Me.AddCardRangeToHand(dealt);
            await Except(hand).AddCardsToPlayerHand(hand.ToData(), DealCount);
        }

        logger.LogDebug("Finished dealing cards.");
    }

    private async Task UpdateGameState()
    {
        Room.Game.Status = GameStatus.Ongoing;
        await Everyone.UpdateGameStatus(Room.Game.Status);
    }
}