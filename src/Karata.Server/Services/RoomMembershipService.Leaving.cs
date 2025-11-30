using Karata.Kit.Domain.Models;
using static Karata.Kit.Domain.Models.HandStatus;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync(string connection, HandStatus intent)
    {
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var player = (await context.Users.FindAsync(CurrentPlayerId))!;
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
                
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());
                
                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).RemoveHandFromRoom(hand.Player.ToData());
                await RoomSpectators.RemoveHandFromRoom(hand.Player.ToData());
                break;
            case GameStatus.Ongoing:
                hand.Status = intent;
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());

                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(hand.Player.ToData(), hand.Status);
                await RoomSpectators.RemoveHandFromRoom(hand.Player.ToData());
                break;
            case GameStatus.Over:
                hand.Status = Away;
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());

                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(hand.Player.ToData(), hand.Status);
                await RoomSpectators.RemoveHandFromRoom(hand.Player.ToData());
                break;
        }
        
        await context.SaveChangesAsync();
    }
}