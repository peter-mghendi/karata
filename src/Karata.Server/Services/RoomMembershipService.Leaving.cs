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
        await RedelegateAdministration(room,  hand);
        RemovePresence(room, hand);

        await context.SaveChangesAsync();
        await NotifyPlayerLeft(room, hand);
    }

    private void ValidateLeavingGameState(Room room)
    {
        // No one can leave an Ongoing game.
        // TODO: Handle this gracefully, as well as accidental disconnection.
        if (room.Game.Status is GameStatus.Ongoing) throw new SawException();
    }

    private async Task RedelegateAdministration(Room room, Hand hand)
    {
        if (room.Administrator.Id != hand.Player.Id) return;

        var remaining = room.Game.Hands.Where(h => h.Player.Id != hand.Player.Id).OrderBy(h => h.Id).ToList();
        if (remaining.Count == 0) return;
        
        room.Administrator = remaining.First().Player;
        await Room.UpdateAdministrator(room.Administrator.ToData());
    }

    private void RemovePresence(Room room, Hand hand)
    {
        room.Game.Hands.Remove(hand);
        presence.RemovePresence(CurrentPlayerId, room.Id.ToString());
    }

    // I'm not removing the user from the SignalR group because this happens automatically on disconnection
    // ref: https://learn.microsoft.com/en-us/aspnet/core/signalr/groups?view=aspnetcore-9.0#add-or-remove-connections-from-a-group
    private async Task NotifyPlayerLeft(Room room, Hand hand)
    {
        await Me.RemoveFromRoom();
        await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).RemoveHandFromRoom(hand.Player.ToData());
    }
}