using System.Reactive.Disposables;
using System.Reactive.Linq;
using Karata.Bot.Strategy;
using Karata.Cards.Extensions;
using Karata.Pebble.Interceptors;
using Karata.Shared.Engine;
using Karata.Shared.Engine.Exceptions;
using Karata.Shared.Models;
using Karata.Shared.State;
using Microsoft.Extensions.Logging.Abstractions;
using static Karata.Shared.Models.GameStatus;

namespace Karata.Bot.Services;

public sealed class BotSession(
    UserData player,
    PlayerConnection realtime,
    IBotStrategy strategy,
    IKarataEngine engine,
    ILoggerFactory loggers
) : IAsyncDisposable
{
    private readonly ILogger<BotSession> _log = loggers.CreateLogger<BotSession>();
    private readonly CompositeDisposable _subs = new();

    private RoomState? _room;
    private TurnState _turn = new([], []);
    private RoomStreamsStateBinder? _binder;

    private UserData Player { get; } = player;
    public HandData? CurrentHand => _room is null ? null : TryGetMyHand(_room.State, Player.Id);

    public async Task StartAsync(CancellationToken ct)
    {
        realtime.Streams.AddToRoom
            .Subscribe(r =>
            {
                _log.LogInformation("I'm joining room {Room} as {Player}. Ready to bring the pain.", r.Id, Player);
                
                _room = new RoomState(r, [
                    new LoggingInterceptor<RoomData>(loggers),
                    new TimingInterceptor<RoomData>(loggers)
                ]);
                _turn = new TurnState([], []);

                _binder = new RoomStreamsStateBinder(_room, _turn, realtime.Streams, NullLoggerFactory.Instance, Player.Id);
            })
            .AddTo(_subs);
        
        realtime.Streams.UpdateGameStatus
            .Where(_ => _room?.State.Game.Status is Ongoing)
            .Subscribe(_ => realtime.SendChat("gg"))
            .AddTo(_subs);

        realtime.Streams.AddToRoom
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Subscribe(_ => PlayTurnSafeAsync(CancellationToken.None))
            .AddTo(_subs);

        realtime.Streams.UpdateGameStatus
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Subscribe(_ => PlayTurnSafeAsync(CancellationToken.None))
            .AddTo(_subs);

        realtime.Streams.ReceiveChat
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Subscribe(_ => PlayTurnSafeAsync(CancellationToken.None))
            .AddTo(_subs);

        realtime.Streams.UpdateTurn
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Subscribe(_ => PlayTurnSafeAsync(CancellationToken.None))
            .AddTo(_subs);

        realtime.Streams.ReceiveSystemMessage
            .Subscribe(message => _log.LogInformation("{Message}", message))
            .AddTo(_subs);

        await realtime.StartAsync(specific =>
        {
            var hand = TryGetMyHand(_room!.State, Player.Id);
            var request = strategy.Request(_room.State, hand.Cards, specific);
            
            _log.LogInformation("I have to request a card. I will request {Card}.", request);
            return request;
        }, ct);
    }

    private static HandData TryGetMyHand(RoomData room, string selfId)
        => room.Game.Hands.First(h => h.Player.Id == selfId);

    private async Task PlayTurnSafeAsync(CancellationToken ct)
    {
        try
        {
            await PlayTurnAsync(ct);
        }
        catch (Exception e)
        {
            _ = e;
        }
    }

    private async Task PlayTurnAsync(CancellationToken ct)
    {
        var room = _room!.State;
        var my = TryGetMyHand(room, Player.Id).Cards;
        _log.LogInformation("It's my turn. I have {Cards}.", string.Join(", ", my.Select(m => m.GetName())));

        var move = strategy.Decide(room, my);
        _log.LogInformation("I'm thinking of playing {Move}.", string.Join(", ", move.Select(m => m.GetName())));

        try
        {
            _ = engine.EvaluateTurn(room.Game, move);
            await realtime.PerformTurn(move, ct);
            await realtime.SendChat("get rekt", ct);
        }
        catch (KarataEngineException)
        {
            await realtime.PerformTurn([], ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _subs.Dispose();
        _binder?.Dispose();
        await realtime.StopAsync();
    }
}