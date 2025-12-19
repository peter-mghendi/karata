using Karata.Kit.Core.Exceptions;
using Karata.Server.Data;
using Karata.Server.Hubs.Clients;
using Karata.Server.Services;
using Karata.Server.Support;
using Karata.Server.Support.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static Karata.Kit.Domain.Models.GameStatus;

namespace Karata.Server.Hubs;

[Authorize]
public class ReplayerHub(ILogger<SpectatorHub> logger, KarataContext context, ReplayProcessor processor) : Hub<IReplayerClient>
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(50);
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        await Clients.Client(Context.ConnectionId).RemoveFromRoom();
    }

    public async Task JoinRoom(string roomId)
    {
        try
        {
            logger.LogDebug("Spectator {Connection} is joining room {Room}.", Context.ConnectionId, roomId);
            if (await context.Rooms.FindAsync(Guid.Parse(roomId)) is not { } room) return;
            if (room.Game.Status is Lobby or Ongoing) throw new GameOngoingException();

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Client(Context.ConnectionId).AddToRoom(room.ToData());
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }

    public async Task Start(string roomId, TimeSpan interval)
    {
        try
        {
            if (interval < MinimumInterval) throw new KarataException("Interval too small");

            if (!Guid.TryParse(roomId, out var roomGuid)) throw new KarataException("Invalid Room ID");
            
            if (await context.Rooms.FindAsync(roomGuid) is not { } room) throw new KarataException("Room not found");

            if (room.Game.Status is not Over) throw new GameOngoingException();

            if (room.Game.Hands.All(hand => hand.Player.Id != Context.UserIdentifier))
                throw new KarataException("You cannot access this replay");

            var request = new ReplayRequest(
                RoomId: roomGuid,
                UserId: Context.UserIdentifier!,
                Interval: interval,
                StartTurn: 0
            );

            await processor.StartAsync(request);
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
        }
        catch (KarataException exception)
        {
            await Clients.Caller.ReceiveSystemMessage(Messages.Exception(exception));
        }
    }
}