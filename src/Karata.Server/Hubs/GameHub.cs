using Karata.Server.Data;
using Karata.Server.Exceptions;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Karata.Server.Services;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Guid;

namespace Karata.Server.Hubs;

[Authorize]
public class GameHub(
    ILogger<GameHub> logger,
    KarataContext context,
    PresenceService presence,
    UserManager<User> users
) : Hub<IGameClient>
{
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        var user = await User();
        logger.LogDebug(exception, "User {User} disconnected.", user.UserName);

        if (!presence.TryGetPresence(user.Id, out var rooms) || rooms is null) return;
        await foreach (var room in Rooms(rooms))
        {
            try
            {
                logger.LogDebug("Ending game in room {Room}.", room.Id.ToString());

                room.Game.Status = GameStatus.Over;
                room.Game.Result = new GameResult
                {
                    ResultType = GameResultType.SystemError,
                    ReasonType = MessageType.Error,
                    Reason = $"{user.UserName} disconnected. This game cannot proceed.",
                };

                await context.SaveChangesAsync();
                await Clients.Group(room.Id.ToString()).EndGame();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while trying to end game.");
                throw;
            }
        }
    }

    public async Task SendChat(string roomId, string text)
    {
        var room = await Room(roomId);
        var chat = new Chat { Text = text, Sender = await User() };

        room.Chats.Add(chat);
        await Clients.Group(roomId).ReceiveChat(chat.ToData());
        await context.SaveChangesAsync();
    }

    public async Task JoinRoom([FromServices] RoomMembershipServiceFactory factory, string roomId, string? password)
    {
        try
        {
            logger.LogDebug("User {User} is joining room {Room}.", Context.UserIdentifier, roomId);
            await factory.Create(await Room(roomId), await User(), Context.ConnectionId).JoinAsync(password);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task LeaveRoom([FromServices] RoomMembershipServiceFactory factory, string roomId)
    {
        try
        {
            logger.LogDebug("User {User} is leaving room {Room}.", Context.UserIdentifier, roomId);
            await factory.Create(await Room(roomId), await User(), Context.ConnectionId).LeaveAsync();
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task StartGame([FromServices] GameStartServiceFactory factory, string roomId)
    {
        try
        {
            logger.LogDebug("User {User} is starting the game in room {Room}.", Context.UserIdentifier, roomId);
            await factory.Create(await Room(roomId), await User(), Context.ConnectionId).ExecuteAsync();
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task PerformTurn([FromServices] TurnProcessingServiceFactory factory, string roomId, List<Card> cards)
    {
        try
        {
            logger.LogDebug("User {User} is performing a turn in room {Room}. Cards: {Cards}", Context.UserIdentifier, roomId, string.Join(", ", cards));
            await factory.Create(await Room(roomId), await User(), Context.ConnectionId).ExecuteAsync(cards);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
            await Clients.Caller.NotifyTurnProcessed();
        }
    }

    private async Task<User> User()
    {
        var user = await users.FindByIdAsync(Context.UserIdentifier!);
        if (user is null) throw new Exception("User not found.");
        return user;
    }

    private async Task<Room> Room(string id) => await Room(Parse(id));

    private async Task<Room> Room(Guid id)
    {
        var room = await context.Rooms.FindAsync(id);
        if (room is null) throw new Exception("Room not found.");
        return room;
    }

    private IAsyncEnumerable<Room> Rooms(IEnumerable<string> ids) => Rooms(ids.Select(Parse));

    private IAsyncEnumerable<Room> Rooms(IEnumerable<Guid> ids)
        => context.Rooms.Where(r => ids.Contains(r.Id)).AsAsyncEnumerable();
}