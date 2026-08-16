using Karata.Cards.Support.Exceptions;
using Karata.Kit.Cards.Models;

namespace Karata.Cards.Services;

public partial class RoomMembershipService
{
    public async Task JoinAsync(string connection)
    {
        var player = (await context.Users.FindAsync(CallerPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;

        ValidateMembership(room, player);
        ValidateJoiningGameState(room, player);
        presence.AddPresence(player.Id, room.Id.ToString());
        
        switch (room.Game.Status)
        {
            case GameStatus.Lobby when room.Game.Hands.SingleOrDefault(h => h.Player.Id == player.Id) is { } joined:
                joined.Status = HandStatus.Active;

                await AddToRoom(connection);
                await Caller.AddToRoom(RoomId, Enrich.ForHand(room, joined));
                await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId)).UpdateHandStatus(RoomId, joined.Id, joined.Status);
                await RoomSpectators.UpdateHandStatus(RoomId, joined.Id, joined.Status);
                break;
            case GameStatus.Lobby:
                var hand = new Hand { Player = player, Status = HandStatus.Active };
                room.Game.Hands.Add(hand);

                await AddToRoom(connection);
                await Caller.AddToRoom(RoomId, Enrich.ForHand(room, hand));
                await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId)).AddHandToRoom(RoomId, hand.Id, hand.Player.ToData(), hand.Status);
                await RoomSpectators.AddHandToRoom(RoomId, hand.Id, hand.Player.ToData(), hand.Status);
                break;
            case GameStatus.Ongoing:
                var rejoined = room.Game.Hands.Single(h => h.Player.Id == player.Id);
                rejoined.Status = HandStatus.Active;

                await AddToRoom(connection);
                await Caller.AddToRoom(RoomId, Enrich.ForHand(room, rejoined));
                await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId)).UpdateHandStatus(RoomId, rejoined.Id, rejoined.Status);
                await RoomSpectators.UpdateHandStatus(RoomId, rejoined.Id, rejoined.Status);
                break;
            case GameStatus.Over:
                break;
        }

        await context.SaveChangesAsync();
    }

    private static void ValidateMembership(Room room, User player)
    {
        if (room.Game.Hands.Any(h => h.Player.Id == player.Id))
            return;

        if (room.Visibility is RoomVisibility.Public or RoomVisibility.Unlisted)
            return;

        throw new UnauthorizedActionException();
    }

    private static void ValidateJoiningGameState(Room room, User player)
    {
        switch (room.Game.Status)
        {
            case GameStatus.Lobby when room.Game.Hands.Count >= 4:
                throw new GameFullException();
            case GameStatus.Ongoing when room.Game.Hands.All(h => h.Player != player):
                throw new GameOngoingException();
            case GameStatus.Over:
                throw new GameOverException();
        }
    }
}