using System.Text;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task JoinAsync(string connection, string? password)
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;
        
        ValidateJoiningGameState(room, player, password);
        presence.AddPresence(player.Id, room.Id.ToString());
        
        switch (room.Game.Status)
        {
            case GameStatus.Lobby when room.Game.Hands.SingleOrDefault(h => h.Player.Id == player.Id) is {} joined:
                joined.Status = Connected;

                await AddToRoom(connection);
                await Me.AddToRoom(room.ToData());
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId))
                    .UpdateHandStatus(joined.Player.ToData(), joined.Status);
                break;
            case GameStatus.Lobby:
                var hand = new Hand { Player = player, Status = Connected };
                room.Game.Hands.Add(hand);

                await AddToRoom(connection);
                await Me.AddToRoom(room.ToData());
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId))
                    .AddHandToRoom(hand.Player.ToData(), hand.Status);
                break;
            case GameStatus.Ongoing:
                var rejoined = room.Game.Hands.Single(h => h.Player.Id == player.Id);
                rejoined.Status = Connected;
                
                await AddToRoom(connection);
                await Me.AddToRoom(room.ToData());
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId)).UpdateHandStatus(rejoined.Player.ToData(), Connected);
                break;
            case GameStatus.Over:
                break;
        }


        await context.SaveChangesAsync();
    }

    private void ValidateJoiningGameState(Room room, User player, string? password)
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

        switch (room.Game.Status)
        {
            case GameStatus.Lobby when room.Game.Hands.Count >= 4:
                throw new GameFullException();
            case GameStatus.Ongoing when room.Game.Hands.All(h => h.Player != player):
                throw new GameOngoingException();
            case GameStatus.Over:
                throw new GameOverException();
        }
    }
}