using Karata.Shared.Models;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync(string connection)
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var hand = room.Game.Hands.Single(h => h.Player.Id == player.Id);

        switch (room.Game.Status)
        {
            case GameStatus.Lobby:

                if (room.Game.Hands.Count > 1)
                {
                    await RedelegateAdministration(room, hand);
                    room.Game.Hands.Remove(hand);
                }
                else
                {
                    room.Game.Hands.Single(h => h.Player.Id == player.Id).Status = Disconnected;
                }
                
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());
                
                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).RemoveHandFromRoom(hand.Player.ToData());
                await RoomSpectators.RemoveHandFromRoom(hand.Player.ToData());
                break;
            case GameStatus.Ongoing:
                hand.Status = Disconnected;
                await RedelegateAdministration(room,  hand);
                await RecomputeTurn(room, player);
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());

                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(hand.Player.ToData(), Disconnected);
                await RoomSpectators.RemoveHandFromRoom(hand.Player.ToData());
                break;
            case GameStatus.Over:
                hand.Status = Disconnected;
                presence.RemovePresence(CurrentPlayerId, room.Id.ToString());

                await AddToRoom(connection);
                await Me.RemoveFromRoom();
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(hand.Player.ToData(), Disconnected);
                await RoomSpectators.RemoveHandFromRoom(hand.Player.ToData());
                break;
        }
        
        await context.SaveChangesAsync();
    }

    // TODO: Find some way to be more forgiving with the current player's connection. Timer? Admin prompt?
    private async Task RecomputeTurn(Room room, User player)
    {
        if (room.Game.CurrentHand.Player.Id != player.Id) return;
        if (room.Game.Hands.Count(hand => hand.Status is Connected) <= 1) return;
        
        GameTurns.Advance(room.Game);
        await RoomPlayers.UpdateTurn(room.Game.CurrentTurn);
        await RoomSpectators.UpdateTurn(room.Game.CurrentTurn);
    }

    private async Task RedelegateAdministration(Room room, Hand hand)
    {
        if (room.Administrator.Id != hand.Player.Id) return;
        if (room.NextEligibleAdministrator is not {} administrator) return;
        
        room.Administrator = administrator;
        await RoomPlayers.UpdateAdministrator(room.Administrator.ToData());
        await RoomSpectators.UpdateAdministrator(room.Administrator.ToData());
    }
}