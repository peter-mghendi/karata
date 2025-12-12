using System.Collections.Concurrent;
using Karata.Kit.Bot.Infrastructure.Security;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Karata.Kit.Bot.Services;

public sealed class BotSessionManager(
    BotSessionFactory bots,
    ILogger<BotSessionManager> log,
    AccessTokenProvider tokens
) : IAsyncDisposable
{
    private sealed record Entry(
        Guid Room,
        DateTimeOffset StartedAt,
        CancellationTokenSource Cancellation,
        Task Runner,
        BotSession Session
    );

    private readonly ConcurrentDictionary<Guid, Entry> _sessions = new();

    /// <summary>
    /// Starts (or no-ops) a bot session for a room and returns the best-known <see cref="HandData"/> snapshot for the bot.
    /// If the game state hasn't hydrated yet, returns a placeholder with <see cref="HandStatus.Away"/> and empty cards,
    /// using the session's <see cref="AccessTokenProvider.CurrentUser"/> if available.
    /// </summary>
    public Task<HandData> StartAsync(IBotStrategy strategy, Guid room, string? password, CancellationToken ct = default)
    {
        if (_sessions.TryGetValue(room, out var existing)) return Task.FromResult(Snapshot(existing));

        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var session = bots.Create(strategy, tokens.CurrentUser!, room, password);
        var runner = Task.Run(async () =>
        {
            try
            {
                // Keep the session alive; it reacts to SignalR events internally.
                // We don't spin a busy loop; this awaits cancellation cooperatively.
                await session.StartAsync(cancellation.Token).ConfigureAwait(false);
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                log.LogError(ex, "Shutting down bot session for room {RoomId}", room);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Bot session crashed for room {RoomId}", room);
            }
            finally
            {
                try
                {
                    await session.DisposeAsync();
                }
                catch
                {
                    // ignored
                }
            }
        }, CancellationToken.None);

        var entry = new Entry(room, DateTimeOffset.UtcNow, cancellation, runner, session);
        _ = _sessions.TryAdd(room, entry);
        return Task.FromResult(Snapshot(_sessions[room]));
    }

    /// <summary>
    /// Lists current HandData snapshots for all active sessions.
    /// </summary>
    public IReadOnlyList<HandData> List() => _sessions.Values.Select(Snapshot).ToList();

    /// <summary>
    /// Gets the current HandData snapshot for a single room, if running.
    /// </summary>
    public HandData? Get(Guid room) => _sessions.TryGetValue(room, out var e) ? Snapshot(e) : null;

    /// <summary>
    /// Stops a running session for a room.
    /// </summary>
    public async Task<bool> StopAsync(Guid room, CancellationToken ct)
    {
        if (!_sessions.TryRemove(room, out var e)) return false;
        try
        {
            await e.Cancellation.CancelAsync();

            try
            {
                await e.Runner.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
        finally
        {
            e.Cancellation.Dispose();
        }

        return true;
    }

    public async ValueTask DisposeAsync() =>
        await Task.WhenAll(from key in _sessions.Keys select StopAsync(key, CancellationToken.None));

    private HandData Snapshot(Entry e) => e.Session.CurrentHand;
}