using Karata.Shared.Models;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync()
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var hand = room.Game.Hands.Single(h => h.Player.Id == player.Id);

        switch (room.Game.Status)
        {
            case GameStatus.Lobby:
                room.Game.Hands.Remove(hand);
                await RedelegateAdministration(room,  hand);
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());
                
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).RemoveHandFromRoom(hand.Player.ToData());
                break;
            case GameStatus.Ongoing:
                hand.Status = Disconnected;
                await RedelegateAdministration(room,  hand);
                await RecomputeTurn(room, player);
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());


                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(hand.Player.ToData(), Disconnected);
                break;
            case GameStatus.Over:
                hand.Status = Disconnected;
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());
                
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(hand.Player.ToData(), Disconnected);
                break;
        }
        
        await context.SaveChangesAsync();
    }

    // TODO: Find some way to be more forgiving with the current player's connection. Maybe start a timer & cancel?
    private async Task RecomputeTurn(Room room, User player)
    {
        if (room.Game.CurrentHand.Player.Id != player.Id) return;
        if (room.Game.Hands.Count(hand => hand.Status is Connected) <= 1) return;
        
        turns.Advance(room.Game);
        await Room.UpdateTurn(room.Game.CurrentTurn);
    }

    private async Task RedelegateAdministration(Room room, Hand hand)
    {
        if (room.Administrator.Id != hand.Player.Id) return;
        if (room.NextEligibleAdministrator is not {} administrator) return;
        
        room.Administrator = administrator;
        await Room.UpdateAdministrator(room.Administrator.ToData());
    }
}