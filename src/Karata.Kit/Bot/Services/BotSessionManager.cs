using System.Collections.Concurrent;
using Karata.Kit.Bot.Infrastructure.Security;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Microsoft.Extensions.Logging;
using static System.Threading.CancellationToken;
using static System.Threading.Tasks.Task;

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
    public async Task StartAsync(IBotStrategy strategy, Guid room, string? password, CancellationToken ct = default)
    {
        if (_sessions.TryGetValue(room, out _)) return;

        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var session = bots.Create(strategy, (await tokens.CurrentUser())!, room, password);
        var runner = Run(() => StartBotSession(room, password, session, cancellation.Token), cancellation.Token);

        var entry = new Entry(room, DateTimeOffset.UtcNow, cancellation, runner, session);
        _ = _sessions.TryAdd(room, entry);
    }

    private async Task StartBotSession(Guid roomId, string? password, BotSession session, CancellationToken cancellation)
    {
        try
        {
            log.LogInformation("Starting bot session for room {RoomId}", roomId);
            
            // Keep the session alive; it reacts to SignalR events internally.
            // We don't spin a busy loop; this awaits cancellation cooperatively.
            await session.StartAsync(roomId, password, cancellation).ConfigureAwait(false);
            await Delay(Timeout.InfiniteTimeSpan, cancellation).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            log.LogError(ex, "Shutting down bot session for room {RoomId}", roomId);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Bot session crashed for room {RoomId}", roomId);
        }
        finally
        {
            try
            {
                session.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }

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

    public async ValueTask DisposeAsync() => await WhenAll(from key in _sessions.Keys select StopAsync(key, None));
}