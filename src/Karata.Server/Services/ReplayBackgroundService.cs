using System.Collections.Concurrent;
using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

sealed record ReplayKey(Guid RoomId, string UserId);

sealed record ReplaySessionHandle(Task Task, CancellationTokenSource Cancellation);

public sealed class ReplayProcessor(IServiceScopeFactory factory, IHubContext<ReplayerHub, IReplayerClient> replayers)
{
    private readonly ConcurrentDictionary<ReplayKey, ReplaySessionHandle> _sessions = new();
    
    public async Task StartAsync(ReplayRequest request)
    {
        using var scope = factory.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<KarataContext>();
        
        var key = new ReplayKey(request.RoomId, request.UserId);
        if (_sessions.TryRemove(key, out var existing))
        {
            await existing.Cancellation.CancelAsync();
        }
        
        var replayer = replayers.Clients.User(request.UserId);
        var room = await context.Rooms.FindAsync(request.RoomId);
        var turns = room!.Game.Hands.SelectMany(hand => hand.Turns).OrderBy(turn => turn.CreatedAt);
        
        var cts = new CancellationTokenSource();
        var runner = new ReplaySessionRunner([..turns], replayer, request.Interval, cts.Token);
        var task = Task.Run(() => runner.RunAsync(), cts.Token);

        _sessions[key] = new ReplaySessionHandle(task, cts);

        await Task.CompletedTask;
    }
}