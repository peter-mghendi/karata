using System.Text;
using Karata.Server.Support;
using Karata.Server.Support.Exceptions;
using Karata.Shared.Models;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Services;

public partial class RoomMembershipService
{
    public async Task JoinAsync(string connection)
    {
        var player = (await users.FindByIdAsync(CurrentPlayerId))!;
        var room = (await context.Rooms.FindAsync(RoomId))!;

        bool authorized;
        do authorized = await VerifyPassword(room, player, connection);
        while (!authorized);

        ValidateJoiningGameState(room, player);
        presence.AddPresence(player.Id, room.Id.ToString());

        var cards = BuildCardMap(room, player);
        
        switch (room.Game.Status)
        {
            case GameStatus.Lobby when room.Game.Hands.SingleOrDefault(h => h.Player.Id == player.Id) is { } joined:
                joined.Status = Connected;

                await AddToRoom(connection);
                await Me.AddToRoom(room.ToData(), cards);
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId))
                    .UpdateHandStatus(joined.Player.ToData(), joined.Status);
                await RoomSpectators.UpdateHandStatus(joined.Player.ToData(), joined.Status);
                break;
            case GameStatus.Lobby:
                var hand = new Hand { Player = player, Status = Connected };
                room.Game.Hands.Add(hand);

                await AddToRoom(connection);
                await Me.AddToRoom(room.ToData(), cards);
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId))
                    .AddHandToRoom(hand.Player.ToData(), hand.Status);
                await RoomSpectators.AddHandToRoom(hand.Player.ToData(), hand.Status);
                break;
            case GameStatus.Ongoing:
                var rejoined = room.Game.Hands.Single(h => h.Player.Id == player.Id);
                rejoined.Status = Connected;

                await AddToRoom(connection);
                await Me.AddToRoom(room.ToData(), cards);
                await Hands(room.Game.HandsExceptPlayerId(CurrentPlayerId))
                    .UpdateHandStatus(rejoined.Player.ToData(), rejoined.Status);
                await RoomSpectators.UpdateHandStatus(rejoined.Player.ToData(), rejoined.Status);
                break;
            case GameStatus.Over:
                break;
        }

        await context.SaveChangesAsync();
    }

    private async Task<bool> VerifyPassword(Room room, User player, string connection)
    {
        try
        {
            if (room.Hash is null) return true;
            if (room.Game.Hands.Any(h => h.Player.Id == player.Id)) return true;

            if (await PlayerConnection(connection).PromptPasscode() is not [_, ..] password)
                throw new PasswordRequiredException();
            if (!passwords.VerifyPassword(Encoding.UTF8.GetBytes(password), room.Salt!, room.Hash))
                throw new IncorrectPasswordException();

            return true;
        }
        catch (PasswordException exception)
        {
            await Me.ReceiveSystemMessage(Messages.Exception(exception));
            return false;
        }
    }

    private static void ValidateJoiningGameState(Room room, User player)
    {
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