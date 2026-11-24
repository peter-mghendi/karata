using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Karata.Cards.Support.Exceptions;
using Karata.Kit.Cards.Models;
using Microsoft.AspNetCore.SignalR;
using static Karata.Kit.Cards.Models.GameStatus;

namespace Karata.Cards.Services;

public class GameStartService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILogger<GameStartService> logger,
    CardsContext context,
    Guid roomId,
    string player
) : LiveRoomAwareService(players, spectators, roomId, player)
{
    private const int DealCount = 4;
    
    public async Task ExecuteAsync()
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;

        ValidateGameState(room);
        var lapsed = RemoveInvitations(room);
        DealCards(room);
        room.Game.Status = Ongoing;

        await context.SaveChangesAsync();
        await NotifyAsync(room, lapsed);
    }
    
    private void ValidateGameState(Room room)
    {
        // Check caller role
        if (room.Administrator.Id != CallerPlayerId)
        {
            throw new UnauthorizedActionException();
        }

        // Check game status
        if (room.Game.Status is not Lobby)
        {
            throw new GameOngoingException();
        }

        // Check player number. Invited players who haven't joined don't count.
        if (room.Game.Hands.Count(hand => hand.Status is HandStatus.Active) is < 2 or > 4)
        {
            throw new NotEnoughPlayersException();
        }
    }

    // Invitations that haven't been accepted lapse when the game starts: those players aren't in it.
    private static List<Hand> RemoveInvitations(Room room)
    {
        var lapsed = room.Game.Hands.Where(hand => hand.Status is HandStatus.Invited).ToList();
        foreach (var hand in lapsed) room.Game.Hands.Remove(hand);

        return lapsed;
    }

    private void DealCards(Room room)
    {
        var game = room.Game;
        var deck = game.Deck;

        // Shuffle deck and deal starting card. Do not deal special cards to start.
        do deck.ShuffleInPlace(); while (deck.Peek().IsSpecial);
        game.Pile.Push(deck.Deal());

        foreach (var hand in game.Hands)
        {
            var dealt = deck.DealMany(DealCount);
            logger.LogDebug("Dealing {Count} cards to {User}. Cards: {Cards}.", DealCount, hand.Player.Username, string.Join(", ", dealt));

            hand.Turns.Add(new Turn { CardsPicked = dealt, Type = TurnType.Deal, Hand = hand, CreatedAt = DateTimeOffset.UtcNow });
            foreach (var card in dealt) hand.Cards.Add(card);
        }

        logger.LogDebug("Finished dealing cards.");
    }

    private async Task NotifyAsync(Room room, List<Hand> lapsed)
    {
        var game = room.Game;
        var top = game.Pile.Peek();

        foreach (var hand in lapsed)
        {
            await RoomPlayers.RemoveHandFromRoom(RoomId, hand.Id);
            await RoomSpectators.RemoveHandFromRoom(RoomId, hand.Id);
        }

        await RoomPlayers.MoveCardsFromDeckToPile(RoomId, [top]);
        await RoomSpectators.MoveCardsFromDeckToPile(RoomId, [top]);

        foreach (var hand in game.Hands)
        {
            var dealt = hand.Turns.First(turn => turn.Type is TurnType.Deal).CardsPicked;
            var dummies = Enumerable.Repeat(new Card(), dealt.Count).ToList();

            await Hand(hand).MoveCardsFromDeckToHand(RoomId, hand.Id, dealt);
            await Hands(game.HandsExceptPlayerId(hand.Player.Id)).MoveCardsFromDeckToHand(RoomId, hand.Id, dummies);
            await RoomSpectators.MoveCardsFromDeckToHand(RoomId, hand.Id, dummies);
        }

        foreach (var hand in game.Hands) await Hand(hand).UpdateGameStatus(RoomId, Enrich.ForHand(game, hand));
        await RoomSpectators.UpdateGameStatus(RoomId, game);
    }
}