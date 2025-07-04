using System.Text;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task JoinAsync(string? password)
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;
        var hand = new Hand { Player = player };
        
        ValidateJoiningGameState(room, password);
        AddPresence(room, hand);
        
        await NotifyPlayerJoined(room, hand);
        await context.SaveChangesAsync();
    }

    private void ValidateJoiningGameState(Room room, string? password)
    {
        if (room.Hash is not null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new PasswordRequiredException();
            }

            if (!passwords.VerifyPassword(Encoding.UTF8.GetBytes(password), room.Salt!, room.Hash))
            {
                throw new IncorrectPasswordException();
            }
        }

        // Check game status
        if (room.Game.Status == GameStatus.Ongoing)
        {
            throw new GameOngoingException();
        }

        // Check player count
        if (room.Game.Hands.Count >= 4)
        {
            throw new GameFullException();
        }
    }

    private void AddPresence(Room room, Hand hand)
    {
        presence.AddPresence(hand.Player.Id, room.Id.ToString());

        if (room.Game.Hands.All(h => h.Player.Id != hand.Player.Id))
        {
            room.Game.Hands.Add(hand);
        }
    }

    private async Task NotifyPlayerJoined(Room room, Hand hand)
    {
        await AddToRoom(ConnectionId);
        await Me.AddToRoom(room.ToData());
        await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).AddHandToRoom(hand.Player.ToData());
    }
}