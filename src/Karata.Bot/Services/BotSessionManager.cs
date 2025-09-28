using System.Collections.Concurrent;
using Karata.Bot.Infrastructure.Security;
using Karata.Shared.Models;

namespace Karata.Bot.Services;

public sealed class BotSessionManager(
    BotSessionFactory bots,
    IHttpClientFactory factory,
    ILogger<BotSessionManager> log,
    KeycloakAccessTokenProvider tokens
) : IDisposable
{
    private sealed record Entry(
        string RoomId,
        DateTimeOffset StartedAt,
        CancellationTokenSource Cts,
        Task RunnerTask,
        BotSession Session
    );

    private readonly ConcurrentDictionary<string, Entry> _sessions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Starts (or no-ops) a bot session for a room and returns the best-known HandData snapshot for the bot.
    /// If the game state hasn't hydrated yet, returns a placeholder with Status = Away and empty cards,
    /// using the session's SelfUser if available.
    /// </summary>
    public async Task<HandData> StartAsync(string roomId, string? password, CancellationToken ct = default)
    {
        if (_sessions.TryGetValue(roomId, out var existing))
            return Snapshot(existing);

        using var http = factory.CreateClient("KarataClient");
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var player = tokens.CurrentUser!;
        var session = bots.Create(player, roomId, password);

        var startedAt = DateTimeOffset.UtcNow;
        var entry = new Entry(roomId, startedAt, cts, Task.CompletedTask, session);
        if (!_sessions.TryAdd(roomId, entry))
            return Snapshot(_sessions[roomId]);

        var runner = Task.Run(async () =>
        {
            try
            {
                await session.StartAsync(cts.Token).ConfigureAwait(false);

                // Keep the session alive; it reacts to SignalR events internally.
                // We don't spin a busy loop; this awaits cancellation cooperatively.
                await Task.Delay(Timeout.InfiniteTimeSpan, cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Bot session crashed for room {RoomId}", roomId);
            }
            finally
            {
                try
                {
                    await session.DisposeAsync();
                }
                catch
                {
                    /* swallow */
                }
            }
        }, CancellationToken.None);

        _sessions[roomId] = entry with { RunnerTask = runner };
        return Snapshot(_sessions[roomId]);
    }

    /// <summary>
    /// Lists current HandData snapshots for all active sessions.
    /// </summary>
    public IReadOnlyList<HandData> List()
        => _sessions.Values
            .Select(Snapshot)
            .ToList();

    /// <summary>
    /// Gets the current HandData snapshot for a single room, if running.
    /// </summary>
    public HandData? Get(string roomId)
        => _sessions.TryGetValue(roomId, out var e) ? Snapshot(e) : null;

    /// <summary>
    /// Stops a running session for a room.
    /// </summary>
    public async Task<bool> StopAsync(string roomId, CancellationToken ct)
    {
        if (!_sessions.TryRemove(roomId, out var e)) return false;
        try
        {
            await e.Cts.CancelAsync();

            try
            {
                await e.RunnerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
        finally
        {
            e.Cts.Dispose();
        }

        return true;
    }

    public void Dispose()
    {
        foreach (var kv in _sessions.Keys) _ = StopAsync(kv, CancellationToken.None);
    }

    private HandData Snapshot(Entry e)
    {
        return e.Session.CurrentHand ?? new HandData
        {
            Id = 0,
            Status = HandStatus.Away,
            Player = tokens.CurrentUser!,
        };
    }
}