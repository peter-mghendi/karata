using System.Reactive;
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
using static Karata.Kit.Domain.Models.GameStatus;

namespace Karata.Kit.Bot.Services;

public sealed class BotSession(
    UserData bot,
    PlayerConnection connection,
    IBotStrategy strategy,
    IKarataEngine engine,
    ILoggerFactory loggers
) : IDisposable
{
    private readonly ILogger<BotSession> _log = loggers.CreateLogger<BotSession>();
    private readonly CompositeDisposable _subscriptions = new();
    private PlayerConnection.Session? _session;

    private RoomStore? _room;
    private HandData? BotHand => _room?.State.Game.Hands.FirstOrDefault(h => h.Player.Id == bot.Id);

    public async Task StartAsync(Guid roomId, string? password, CancellationToken cancellation)
    {
        _log.LogInformation("I'm alive! Initializing...");
          
        var parameters = new PlayerConnection.SessionParameters(
            
            OnRequestPassword: () => Task.FromResult(password),
            OnRequestCard: specific =>
            {
                var request = strategy.Request(_room!.State, BotHand!.Cards, specific);

                _log.LogInformation("I have to request a card. I will request {Card}.", request);
                return Task.FromResult(request);
            },
            OnRequestLastCard: () => Task.FromResult(true) 
        );
        _session = connection.Spawn(roomId, parameters);
        
        _session.Events.AddToRoom
            .Subscribe(r =>
            {
                _room = new RoomStore(r, [
                    new LoggingInterceptor<RoomData>(loggers),
                    new TimingInterceptor<RoomData>(loggers)
                ]);
                
                _session.Events.BindRoomState(_room).DisposeWith(_subscriptions);
                _session.Events.AddToRoom.Select(_ => Unit.Default)
                    .Merge(_session.Events.TurnCommitted.Select(_ => Unit.Default))
                    .WithLatestFrom(_room!.Changes, (_, room) => room)
                    .Where(CanPlay)
                    .Select(_ => Observable.FromAsync(PlayTurnSafeAsync))
                    .Concat()
                    .Subscribe()
                    .DisposeWith(_subscriptions);
                
                _room.Changes
                    .Where(room => room.Game.Status is Ongoing)
                    .Take(1)
                    .Select(_ => Observable.FromAsync(async _ => await _session.SendChat("gg", cancellation)))
                    .Concat()
                    .Subscribe()
                    .DisposeWith(_subscriptions);

                _log.LogInformation("I've joined room {Room} as {Player}. Ready to bring the pain.", r.Id, BotHand!.Username);  
            })
            .DisposeWith(_subscriptions);

        _session.Events.SystemMessage
            .Subscribe(message => _log.LogInformation("SYSTEM: {Message}", message))
            .DisposeWith(_subscriptions);
        
        _log.LogInformation("Nearly there...");
        await _session.JoinRoom(cancellation);
        await _session.SendChat("hi guys", cancellation);
        
        _log.LogInformation("Done");
    }

    private bool CanPlay(RoomData room) => room.Game.Status is Ongoing && room.Game.CurrentHand.Player.Id == bot.Id;

    private async Task PlayTurnSafeAsync(CancellationToken ct)
    {
        try
        {
            await PlayTurnAsync(ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Could not perform turn.");
        }
    }

    private async Task PlayTurnAsync(CancellationToken ct)
    {
        if (_session is null) throw new InvalidOperationException("Session is uninitialized.");
            
        _log.LogInformation("It's my turn. I have {Cards}.", string.Join(", ", BotHand!.Cards.Select(card => card.Name)));

        var room = _room!.State;
        var move = strategy.Decide(room, BotHand!.Cards);
        _log.LogInformation("I will play {Move}.", string.Join(", ", move.Select(card => card.Name)));

        try
        {
            // Pre-validate turn, discard delta.
            _ = engine.EvaluateTurn(room.Game, move);

            await _session.PerformTurn(move, ct);
            await _session.SendChat("get rekt", ct);

            _log.LogInformation("I have played {Move}.", string.Join(", ", move.Select(card => card.Name)));
        }
        catch (KarataEngineException)
        {
            // Fallback in case the strategy returns an invalid move.
            // TODO: [Fairness] Make this less punitive. Give bot chance to try again?
            await _session.PerformTurn([], ct);
            _log.LogInformation("I have played a blank turn.");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Could not perform turn {Cards}.", string.Join(',', move));
        }
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        _session?.Dispose();
    }
}