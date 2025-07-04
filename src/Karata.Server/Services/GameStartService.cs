using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class GameStartService(
    IHubContext<GameHub, IGameClient> hub,
    ILogger<GameStartService> logger,
    KarataContext context,
    Guid room,
    string player,
    string connection
) : HubAwareService(hub, room, player, connection)
{
    private const int DealCount = 4;
    
    public async Task ExecuteAsync()
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        
        ValidateGameState(room);
        await DealCards(room);
        await UpdateGameState(room);
        await context.SaveChangesAsync();
    }
    
    private void ValidateGameState(Room room)
    {
        var game = room.Game;
        
        // Check caller role
        if (room.Creator.Id != CurrentPlayerId)
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

    private async Task DealCards(Room room)
    {
        // Shuffle deck and deal starting card
        var game = room.Game;
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
        foreach (var hand in game.Hands)
        {
            var dealt = deck.DealMany(DealCount);

            logger.LogDebug("Dealing {Count} cards to {User}. Cards: {Cards}.", DealCount, hand.Player.UserName, string.Join(", ", dealt));

            hand.Cards.AddRange(dealt);
            await Hand(hand).MoveCardsFromDeckToHand(dealt);
            await Hands(room.Game.HandsExceptPlayerId(hand.Player.Id)).MoveCardCountFromDeckToHand(hand.Player.ToData(), DealCount);
        }

        logger.LogDebug("Finished dealing cards.");
    }

    private async Task UpdateGameState(Room room)
    {
        room.Game.Status = GameStatus.Ongoing;
        await Everyone.UpdateGameStatus(room.Game.Status);
    }
}