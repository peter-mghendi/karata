using Karata.Kit.Cards.Models;

namespace Karata.Cards.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync(string connection)
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var hand = room.Game.Hands.Single(h => h.Player.Id == CallerPlayerId);

        var removed = room.Game.Status is GameStatus.Lobby && room.Game.Hands.Count > 1;
        if (removed) room.Game.Hands.Remove(hand);
        else hand.Status = HandStatus.Inactive;

        await context.SaveChangesAsync();

        presence.RemovePresence(CallerPlayerId, room.Id.ToString());
        await RemoveFromRoom(connection);
        await Caller.RemoveFromRoom(RoomId);

        var others = Hands(room.Game.HandsExceptPlayerId(CallerPlayerId));
        if (removed)
        {
            await others.RemoveHandFromRoom(RoomId, hand.Id);
            await RoomSpectators.RemoveHandFromRoom(RoomId, hand.Id);
        }
        else
        {
            await others.UpdateHandStatus(RoomId, hand.Id, hand.Status);
            await RoomSpectators.UpdateHandStatus(RoomId, hand.Id, hand.Status);
        }
    }
}