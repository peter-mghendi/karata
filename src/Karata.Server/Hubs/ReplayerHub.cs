using Humanizer;
using Karata.Kit.Core.Exceptions;
using Karata.Server.Data;
using Karata.Server.Hubs.Clients;
using Karata.Server.Services;
using Karata.Server.Support.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using static Karata.Kit.Domain.Models.GameStatus;

namespace Karata.Server.Hubs;

[Authorize]
public partial class ReplayerHub(ILogger<SpectatorHub> logger, KarataContext context, ReplayProcessor processor) : Hub<IReplayerClient>
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(50);

    public async Task JoinRoom(Guid roomId)
    {
        try
        {
            LogSpectatorAction(logger, Context.ConnectionId, "joining", roomId);
            if (await context.Rooms.FindAsync(roomId) is not { } room) return;
            if (room.Game.Status is Lobby or Ongoing) throw new GameOngoingException();

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Client(Context.ConnectionId).AddToRoom(roomId, room.ToData());
        }
        catch (KarataException exception)
        {
            await Clients.Caller.SystemMessage(roomId, exception.SystemMessage);
        }
    }

    public async Task Start(Guid roomId, TimeSpan interval)
    {
        try
        {
            if (interval < MinimumInterval)
                throw new KarataException($"Interval must be greater than {MinimumInterval.Humanize()}");
            
            if (await context.Rooms.FindAsync(roomId) is not { } room)
                throw new KarataException("Room not found");

            if (room.Game.Status is not Over)
                throw new GameOngoingException();

            if (room.Game.Hands.All(hand => hand.Player.Id != Context.UserIdentifier))
                throw new KarataException("You cannot access this replay");

            var request = new ReplayRequest(
                RoomId: roomId,
                UserId: Context.UserIdentifier!,
                Interval: interval,
                StartTurn: 0
            );

            await processor.StartAsync(request);
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
            LogSpectatorAction(logger, Context.ConnectionId, "leaving", roomId);
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Client(Context.ConnectionId).RemoveFromRoom(roomId);
        }
        catch (KarataException exception)
        {
            await Clients.Caller.SystemMessage(roomId, exception.SystemMessage);
        }
    }

    [LoggerMessage(LogLevel.Debug, "Spectator {spectator} is {action} room {room}.")]
    private static partial void LogSpectatorAction(ILogger<SpectatorHub> logger, string spectator, string action, Guid room);
}