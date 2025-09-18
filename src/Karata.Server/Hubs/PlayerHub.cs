using Karata.Server.Data;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Karata.Server.Services;
using Karata.Shared.Exceptions;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static System.Guid;

namespace Karata.Server.Hubs;

[Authorize]
public class PlayerHub(
    ILogger<PlayerHub> logger,
    KarataContext context,
    PresenceService presence,
    RoomMembershipServiceFactory membership
) : Hub<IPlayerClient>
{
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        if (await context.Users.FindAsync(Context.UserIdentifier!) is not { } user)
            throw new Exception("User not found.");

        logger.LogDebug(exception, "User {User} disconnected.", user.Username);

        if (!presence.TryGetPresence(user.Id, out var rooms) || rooms is null) return;

        var ids = rooms.Select(Parse);
        foreach (var room in await context.Rooms.Where(r => ids.Contains(r.Id)).ToListAsync()) await Disconnect(membership, room.Id, HandStatus.Offline);
    }

    public async Task SendChat(string roomId, string text)
    {
        if (await context.Users.FindAsync(Context.UserIdentifier!) is not { } user)
            throw new Exception("User not found.");

        if (await context.Rooms.FindAsync(Parse(roomId)) is not { } room)
            throw new Exception("Room not found.");

        var chat = new Chat { Text = text, Sender = user, SentAt = DateTimeOffset.UtcNow };

        room.Chats.Add(chat);
        await Clients.Group(roomId).ReceiveChat(chat);
        await context.SaveChangesAsync();
    }

    public async Task JoinRoom([FromServices] RoomMembershipServiceFactory factory, string roomId)
    {
        logger.LogDebug("User {User} is joining room {Room}.", Context.UserIdentifier, roomId);

        try
        {
            await factory.Create(Parse(roomId), Context.UserIdentifier!).JoinAsync(Context.ConnectionId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task LeaveRoom([FromServices] RoomMembershipServiceFactory factory, string roomId) =>
        await Disconnect(factory, Parse(roomId), HandStatus.Away);

    public async Task StartGame([FromServices] GameStartServiceFactory factory, string roomId)
    {
        try
        {
            logger.LogDebug("User {User} is starting the game in room {Room}.", Context.UserIdentifier, roomId);
            await factory.Create(Parse(roomId), Context.UserIdentifier!).ExecuteAsync();
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
            logger.LogDebug("User {User} is performing a turn in room {Room}. Cards: {Cards}", Context.UserIdentifier,
                roomId, string.Join(", ", cards));
            await factory.Create(Parse(roomId), Context.UserIdentifier!, Context.ConnectionId).ExecuteAsync(cards);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
            await Clients.Caller.NotifyTurnProcessed();
        }
    }

    public async Task VoidTurn([FromServices] VoidTurnServiceFactory factory, string roomId, string voideeId)
    {
        try
        {
            logger.LogDebug("User {User} is voidinging a turn in room {Room}. Player: {Player}", Context.UserIdentifier,
                roomId, voideeId);
            await factory.Create(Parse(roomId), Context.UserIdentifier!).ExecuteAsync(voideeId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task SetAway([FromServices] SetAwayServiceFactory factory, string roomId, string voideeId)
    {
        try
        {
            logger.LogDebug("User {User} is setting player - {Player} as away in room {Room}.", Context.UserIdentifier,
                voideeId, roomId);
            await factory.Create(Parse(roomId), Context.UserIdentifier!).ExecuteAsync(voideeId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    private async Task Disconnect(RoomMembershipServiceFactory factory, Guid roomId, HandStatus intent)
    {
        try
        {
            logger.LogDebug("User {User} is leaving room {Room}.", Context.UserIdentifier, roomId);
            await factory.Create(roomId, Context.UserIdentifier!).LeaveAsync(Context.ConnectionId, intent);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }
}