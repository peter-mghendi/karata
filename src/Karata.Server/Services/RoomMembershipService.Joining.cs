using System.Text;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task JoinAsync(string? password)
    {
        ValidateJoiningGameState(password);

        var hand = new Hand { Player = CurrentPlayer };
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

            if (!passwords.VerifyPassword(Encoding.UTF8.GetBytes(password), Room.Salt!, Room.Hash))
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
        await AddToRoom(Client);
        await Me.AddToRoom(Room.ToData());
        await Others.AddHandToRoom(hand.Player.ToData());
    }
}