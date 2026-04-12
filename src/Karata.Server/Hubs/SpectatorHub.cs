using Karata.Kit.Core.Exceptions;
using Karata.Server.Data;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Hubs;

// IMPORTANT! The SpectatorHub can be accessed by unauthenticated users.
// Do not depend on Context.UserIdentifier being present.
public class SpectatorHub(ILogger<SpectatorHub> logger, KarataContext context) : Hub<ISpectatorClient>
{
    public async Task JoinRoom(Guid roomId)
    {
        try
        {
            logger.LogDebug("Spectator {Connection} is joining room {Room}.", Context.ConnectionId, roomId);
            if (await context.Rooms.FindAsync(roomId) is not { } room) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Client(Context.ConnectionId).AddToRoom(roomId, room.ToData());
        }
        catch (KarataException exception)
        {
            await Clients.Caller.SystemMessage(roomId, exception.SystemMessage);
        }
    }

    public async Task LeaveRoom(Guid roomId)
    {
        try
        {
            logger.LogDebug("Spectator {Connection} is leaving room {Room}.", Context.ConnectionId, roomId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Client(Context.ConnectionId).RemoveFromRoom(roomId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.SystemMessage(roomId, exception.SystemMessage);
        }
    }
}