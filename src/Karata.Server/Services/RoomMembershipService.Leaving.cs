using Karata.Kit.Domain.Models;
using static Karata.Kit.Domain.Models.HandStatus;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync(string connection, HandStatus intent)
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var player = (await context.Users.FindAsync(CallerPlayerId))!;
        var hand = room.Game.Hands.Single(h => h.Player.Id == player.Id);

        switch (room.Game.Status)
        {
            case GameStatus.Lobby:

                if (room.Game.Hands.Count > 1)
                {
                    room.Game.Hands.Remove(hand);
                }
                else
                {
                    room.Game.Hands.Single(h => h.Player.Id == player.Id).Status = Away;
                }
                
                presence.RemovePresence(CallerPlayerId, room.Id.ToString());
                
                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId)).RemoveHandFromRoom(hand.Id);
                await RoomSpectators.RemoveHandFromRoom(hand.Id);
                break;
            case GameStatus.Ongoing:
                hand.Status = intent;
                presence.RemovePresence(CallerPlayerId, room.Id.ToString());

                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId)).UpdateHandStatus(hand.Id, hand.Status);
                await RoomSpectators.RemoveHandFromRoom(hand.Id);
                break;
            case GameStatus.Over:
                hand.Status = Away;
                presence.RemovePresence(CallerPlayerId, room.Id.ToString());

                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CallerPlayerId)).UpdateHandStatus(hand.Id, hand.Status);
                await RoomSpectators.RemoveHandFromRoom(hand.Id);
                break;
        }
        
        await context.SaveChangesAsync();
    }
}