using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task LeaveAsync()
    {
        ValidateLeavingGameState();

        var hand = Game.Hands.Single(h => h.Player.Id == user.Id);
        RemovePresence(hand);
        await NotifyPlayerLeft(hand);
        await context.SaveChangesAsync();
    }

    private void ValidateLeavingGameState()
    {
        // Room creator can not leave games in Lobby.
        // No one can leave an Ongoing game.
        // Anyone can leave if the game is Over.
        if (Game.Status is GameStatus.Ongoing || (Room.Creator.Id == user.Id && Game.Status == GameStatus.Lobby))
        {
            // TODO: Handle this gracefully, as well as accidental disconnection.
            throw new SawException();
        }
    }

    private void RemovePresence(Hand hand)
    {
        Game.Hands.Remove(hand);
        presence.RemovePresence(user.Id, Room.Id.ToString());
    }

    private async Task NotifyPlayerLeft(Hand hand)
    {
        await RemoveFromRoom(hand);
        await Me.RemoveFromRoom();
        await Others.RemoveHandFromRoom(hand.ToData());
    }
}