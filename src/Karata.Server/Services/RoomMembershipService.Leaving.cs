using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync()
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var hand = room.Game.Hands.Single(h => h.Player.Id == player.Id);
        
        ValidateLeavingGameState(room);
        RemovePresence(room, hand);

        await NotifyPlayerLeft(room, hand);
        await context.SaveChangesAsync();
    }

    private void ValidateLeavingGameState(Room room)
    {
        // Room creator can not leave games in Lobby.
        // No one can leave an Ongoing game.
        // Anyone can leave if the game is Over.
        if (room.Game.Status is GameStatus.Ongoing || (room.Creator.Id == CurrentPlayerId && room.Game.Status == GameStatus.Lobby))
        {
            // TODO: Handle this gracefully, as well as accidental disconnection.
            throw new SawException();
        }
    }

    private void RemovePresence(Room room, Hand hand)
    {
        room.Game.Hands.Remove(hand);
        presence.RemovePresence(CurrentPlayerId, room.Id.ToString());
    }

    private async Task NotifyPlayerLeft(Room room, Hand hand)
    {
        await RemoveFromRoom(ConnectionId);
        await Me.RemoveFromRoom();
        await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).RemoveHandFromRoom(hand.Player.ToData());
    }
}