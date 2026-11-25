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

        // Joining the lobby for the first time takes a seat; anyone else already has a hand to come back to.
        var existing = room.Game.Hands.SingleOrDefault(h => h.Player.Id == player.Id);
        var hand = existing ?? new Hand { Player = player, Status = HandStatus.Active };
        if (existing is null) room.Game.Hands.Add(hand);
        hand.Status = HandStatus.Active;

        // Save before telling anyone. A new hand only gets its ID here.
        await context.SaveChangesAsync();

        presence.AddPresence(player.Id, room.Id.ToString());
        await AddToRoom(connection);
        await Caller.AddToRoom(RoomId, Enrich.ForHand(room, hand));

        var others = Hands(room.Game.HandsExceptPlayerId(CallerPlayerId));
        if (existing is null)
        {
            await others.AddHandToRoom(RoomId, hand.Id, hand.Player.ToData(), hand.Status);
            await RoomSpectators.AddHandToRoom(RoomId, hand.Id, hand.Player.ToData(), hand.Status);
        }
        else
        {
            await others.UpdateHandStatus(RoomId, hand.Id, hand.Status);
            await RoomSpectators.UpdateHandStatus(RoomId, hand.Id, hand.Status);
        }
    }

    private static void ValidateMembership(Room room, User player)
    {
        if (room.Game.Hands.Any(h => h.Player.Id == player.Id))
            return;

        if (room.Visibility is RoomVisibility.Public)
            return;

        throw new UnauthorizedActionException();
    }

    private static void ValidateJoiningGameState(Room room, User player)
    {
        var seated = room.Game.Hands.Any(h => h.Player.Id == player.Id);

        switch (room.Game.Status)
        {
            case GameStatus.Lobby when !seated && room.Game.Hands.Count >= 4:
                throw new GameFullException();
            case GameStatus.Ongoing when !seated:
                throw new GameOngoingException();
            case GameStatus.Over:
                throw new GameOverException();
        }
    }
}