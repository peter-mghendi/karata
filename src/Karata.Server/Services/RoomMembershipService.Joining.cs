using System.Text;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task JoinAsync(string? password)
    {
        ValidateJoiningGameState(password);

        var hand = new Hand { Player = user };
        AddPresence(hand);
        await NotifyPlayerJoined(hand);
        await context.SaveChangesAsync();
    }

    private void ValidateJoiningGameState(string? password)
    {
        if (Room.Hash is not null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new PasswordRequiredException();
            }

            var bytes = Encoding.UTF8.GetBytes(password);
            if (!passwords.VerifyPassword(bytes, Room.Salt!, Room.Hash))
            {
                throw new IncorrectPasswordException();
            }
        }

        // Check game status
        if (Game.Status == GameStatus.Ongoing)
        {
            throw new GameOngoingException();
        }

        // Check player count
        if (Game.Hands.Count >= 4)
        {
            throw new GameFullException();
        }
    }

    private void AddPresence(Hand hand)
    {
        presence.AddPresence(hand.Player.Id, Room.Id.ToString());

        if (Game.Hands.All(h => h.Player.Id != hand.Player.Id))
        {
            Game.Hands.Add(hand);
        }
    }

    private async Task NotifyPlayerJoined(Hand hand)
    {
        await AddToRoom(hand);
        await Me.AddToRoom(Room.ToData());
        await Others.AddHandToRoom(hand.ToData());
    }
}