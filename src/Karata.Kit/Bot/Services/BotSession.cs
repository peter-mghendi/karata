using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Karata.Cards.Extensions;
using Karata.Kit.Application.Client.Connection;
using Karata.Kit.Application.Client.State;
using Karata.Kit.Application.Store;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Karata.Kit.Engine.Exceptions;
using Karata.Pebble.Interceptors;
using Microsoft.Extensions.Logging;

namespace Karata.Kit.Bot.Services;

public sealed class BotSession(
    UserData bot,
    IBotStrategy strategy,
    PlayerConnection connection,
    IKarataEngine engine,
    ILoggerFactory loggers
) : IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly ILogger<BotSession> _log = loggers.CreateLogger<BotSession>();
    private RoomStore? _room;
    private TurnPlan? _plan;

    public async Task StartAsync(Guid roomId, string? password, CancellationToken ct)
    {
        var parameters = new PlayerConnection.SessionParameters(
            OnRequestPassword: () => Task.FromResult(password),
            OnRequestCard: specific => Task.FromResult(_plan!.RequestFactory(specific)),
            OnRequestLastCard: () => Task.FromResult(_plan!.LastCardStatusFactory(_room!.State))
        );

        var session = connection.Spawn(roomId, parameters).DisposeWith(_disposables);
        session.Events.AddToRoom
            .Subscribe(r =>
            {
                _room = new RoomStore(r, [
                    new LoggingInterceptor<RoomData>(loggers),
                    new TimingInterceptor<RoomData>(loggers)
                ]);

                _room.Changes
                    .Where(room => room.Game.Status is GameStatus.Ongoing)
                    .Take(1)
                    .Select(_ => Observable.FromAsync(async cancellation => await session.SendChat("gg", cancellation)))
                    .Concat()
                    .Subscribe()
                    .DisposeWith(_disposables);

                session.Events.BindRoomState(_room).DisposeWith(_disposables);
                Observable.Return(_room.State.Game)
                    .Merge(session.Events.UpdateGameStatus)
                    .Merge(session.Events.TurnCommitted)
                    .Where(game => game.Status is GameStatus.Ongoing && game.CurrentHand.Player.Id == bot.Id)
                    .Select(_ => Observable.FromAsync(async cancellation => await PlayTurnAsync(session, cancellation)))
                    .Concat()
                    .Subscribe()
                    .DisposeWith(_disposables);

                _log.LogDebug("Ready to bring the pain in {Room}.", r.Id);
            })
            .DisposeWith(_disposables);

        session.Events.SystemMessage
            .Subscribe(message => _log.LogDebug("SYSTEM: {Message}", message))
            .DisposeWith(_disposables);

        await session.JoinRoom(ct);
        await session.SendChat("behold me, peasants", ct);
    }

    private async Task PlayTurnAsync(PlayerConnection.Session session, CancellationToken ct)
    {
        try
        {
            _plan = await strategy.PlanAsync(_room!.State, ct);
            _ = engine.EvaluateTurn(_room!.State.Game, _plan.Move);
            await session.PerformTurn(_plan.Move, ct);
            await session.SendChat("get rekt", ct);

            _log.LogDebug("Played {Move}.", string.Join(", ", _plan.Move.Select(card => card.Name)));
        }
        catch (KarataEngineException ex)
        {
            await session.PerformTurn([], ct);
            _log.LogError(ex, "Turn invalid. Played empty turn.");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Could not play turn.");
        }
    }

    public void Dispose() => _disposables.Dispose();
}