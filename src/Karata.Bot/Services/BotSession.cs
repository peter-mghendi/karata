using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Karata.Bot.Strategy;
using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Kit.Application.Client;
using Karata.Kit.Application.State;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Karata.Kit.Engine.Exceptions;
using Karata.Pebble.Interceptors;
using static Karata.Kit.Domain.Models.GameStatus;

namespace Karata.Bot.Services;

public sealed class BotSession(
    UserData player,
    PlayerRoomConnection connection,
    IBotStrategy strategy,
    IKarataEngine engine,
    ILoggerFactory loggers
) : IAsyncDisposable
{
    private readonly ILogger<BotSession> _log = loggers.CreateLogger<BotSession>();
    private readonly CompositeDisposable _subscriptions = new();

    private RoomState? _room;
    private TurnState _turn = new([], []);
    private UserData Player { get; } = player;
    public HandData CurrentHand => _room!.State.Game.Hands.First(h => h.Player.Id == Player.Id);

    public async Task StartAsync(CancellationToken cancellation)
    {
        connection.Events.AddToRoom
            .Subscribe(r =>
            {
                _log.LogInformation("I'm joining room {Room} as {Player}. Ready to bring the pain.", r.Id, Player);
                _room = new RoomState(r, [
                    new LoggingInterceptor<RoomData>(loggers),
                    new TimingInterceptor<RoomData>(loggers)
                ]);
                _turn = new TurnState([], [
                    new LoggingInterceptor<ImmutableList<Card>>(loggers),
                    new TimingInterceptor<ImmutableList<Card>>(loggers)
                ]);

                connection.Events.BindRoomState(_room).DisposeWith(_subscriptions);
            })
            .DisposeWith(_subscriptions);

        connection.Events.UpdateGameStatus
            .Where(_ => _room?.State.Game.Status is Ongoing)
            .Select(_ => Observable.FromAsync(async _ => await connection.SendChat("gg", cancellation)))
            .Concat()
            .Subscribe()
            .DisposeWith(_subscriptions);

        connection.Events.AddToRoom
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Select(_ => Observable.FromAsync(async _ => await PlayTurnSafeAsync(CancellationToken.None)))
            .Concat()
            .Subscribe()
            .DisposeWith(_subscriptions);
        
        connection.Events.NotifyTurnProcessed.Subscribe(_ => _turn.Mutate(new TurnState.Clear())).DisposeWith(_subscriptions);

        connection.Events.UpdateGameStatus
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Select(_ => Observable.FromAsync(async _ => await PlayTurnSafeAsync(CancellationToken.None)))
            .Concat()
            .Subscribe()
            .DisposeWith(_subscriptions);

        connection.Events.ReceiveChat
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Select(_ => Observable.FromAsync(async _ => await PlayTurnSafeAsync(CancellationToken.None)))
            .Concat()
            .Subscribe()
            .DisposeWith(_subscriptions);

        connection.Events.UpdateTurn
            .Where(_ => _room?.State.Game.Status is Ongoing && _room.State.Game.CurrentHand.Player.Id == Player.Id)
            .Select(_ => Observable.FromAsync(async _ => await PlayTurnSafeAsync(CancellationToken.None)))
            .Concat()
            .Subscribe()
            .DisposeWith(_subscriptions);

        connection.Events.ReceiveSystemMessage
            .Subscribe(message => _log.LogInformation("{Message}", message))
            .DisposeWith(_subscriptions);

        await connection.StartAsync(
            onRequestCard: specific =>
            {
                var hand = CurrentHand;
                var request = strategy.Request(_room!.State, hand.Cards, specific);

                _log.LogInformation("I have to request a card. I will request {Card}.", request);
                return Task.FromResult(request);
            },
            onRequestLastCard: () => Task.FromResult(true),
            cancellation
        );
        await connection.SendChat("hi guys", cancellation);
    }

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
        var cards = CurrentHand.Cards;
        _log.LogInformation("It's my turn. I have {Cards}.", string.Join(", ", cards.Select(card => card.Name)));

        var room = _room!.State;
        var move = strategy.Decide(room, cards);
        _log.LogInformation("I'm thinking of playing {Move}.", string.Join(", ", move.Select(card => card.Name)));

        try
        {
            _ = engine.EvaluateTurn(room.Game, move);
            await connection.PerformTurn(move, ct);
            await connection.SendChat("get rekt", ct);
        }
        catch (KarataEngineException)
        {
            await connection.PerformTurn([], ct);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _subscriptions.Dispose();
        await connection.StopAsync();
    }
}