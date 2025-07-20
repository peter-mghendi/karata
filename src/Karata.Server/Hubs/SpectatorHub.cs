using Karata.Server.Data;
using Karata.Server.Exceptions;
using Karata.Server.Hubs.Clients;
using Karata.Server.Support;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Hubs;

// IMPORTANT! The SpectatorHub can be accessed by unauthenticated users.
// Do not depend on Context.UserIdentifier being present.
public class SpectatorHub(ILogger<SpectatorHub> logger, KarataContext context) : Hub<ISpectatorClient>
{
    private static readonly HashSet<string> ConnectedSpectators = [];

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        await Clients.Client(Context.ConnectionId).RemoveFromRoom();
        ConnectedSpectators.Remove(Context.ConnectionId);
    }

    public async Task JoinRoom(string roomId)
    {
        try
        {
            logger.LogDebug("Spectator {Connection} is joining room {Room}.", Context.ConnectionId, roomId);
            if (await context.Rooms.FindAsync(Guid.Parse(roomId)) is not { } room) return;
            
            var counts = room.Game.Hands.ToDictionary(h => h.Player.Id, h => h.Cards.Count);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Client(Context.ConnectionId).AddToRoom(room.ToData(), counts);
            ConnectedSpectators.Add(Context.ConnectionId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        try
        {
            logger.LogDebug("Spectator {Connection} is leaving room {Room}.", Context.ConnectionId, roomId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Client(Context.ConnectionId).RemoveFromRoom();
            ConnectedSpectators.Remove(Context.ConnectionId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }
}