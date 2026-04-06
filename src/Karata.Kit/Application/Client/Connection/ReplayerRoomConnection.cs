using Karata.Cards;
using Karata.Kit.Application.Client.State;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Application.Client.Connection;



public sealed class ReplayerRoomConnection(Uri host, Guid roomId) : IRoomConnection<ReplayerRoomConnection.StartParameters>
{
    public sealed record StartParameters(TimeSpan Interval) : IRoomConnectionStartParameters;
    
    private const string HubPath = "/hubs/game/replay";

    public HubConnection? Hub { get; private set; }
    public RoomEvents Events { get; } = new();
    public required Func<Task<string?>> AccessTokenProvider { get; init; }

    public async Task StartAsync(StartParameters parameters, CancellationToken ct = default)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(host, HubPath), o => o.AccessTokenProvider = AccessTokenProvider)
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        Hub.On<RoomData>(nameof(Events.AddToRoom), room => Events.OnAddToRoom(room));
        Hub.On<long, UserData, HandStatus>(nameof(Events.AddHandToRoom), (handId, user, status) => Events.OnAddHandToRoom(handId, user, status));
        Hub.On<TurnResolution>(nameof(Events.TurnCommitted), resolution => Events.OnTurnCommitted(resolution));
        Hub.On<GameResultData>(nameof(Events.EndGame), result => Events.OnEndGame(result));
        Hub.On<long, List<Card>>(nameof(Events.MoveCardsFromDeckToHand), (handId, cards) => Events.OnMoveCardsFromDeckToHand(handId, cards));
        Hub.On<List<Card>>(nameof(Events.MoveCardsFromDeckToPile), cards => Events.OnMoveCardsFromDeckToPile(cards));
        Hub.On<long, List<Card>, bool>(nameof(Events.MoveCardsFromHandToPile), (handId, cards, visible) => Events.OnMoveCardsFromHandToPile(handId, cards, visible));
        Hub.On<SystemMessage>(nameof(Events.SystemMessage), m => Events.OnSystemMessage(m));
        Hub.On(nameof(Events.ReclaimPile), () => Events.OnReclaimPile());
        Hub.On(nameof(Events.RemoveFromRoom), () => Events.OnRemoveFromRoom());
        Hub.On<long>(nameof(Events.RemoveHandFromRoom), handId => Events.OnRemoveHandFromRoom(handId));
        Hub.On<Card>(nameof(Events.SetCurrentRequest), card => Events.OnSetCurrentRequest(card));
        Hub.On<UserData>(nameof(Events.UpdateAdministrator), user => Events.OnUpdateAdministrator(user));
        Hub.On<GameStatus>(nameof(Events.UpdateGameStatus), status => Events.OnUpdateGameStatus(status));
        Hub.On<long, HandStatus>(nameof(Events.UpdateHandStatus), (handId, status) => Events.OnUpdateHandStatus(handId, status));
        Hub.On<uint>(nameof(Events.UpdatePick), pick => Events.OnUpdatePick(pick));
        Hub.On<int>(nameof(Events.UpdateTurn), turn => Events.OnUpdateTurn(turn));

        await Hub.StartAsync(ct);
        await JoinRoom(ct);
        await Start(parameters.Interval, ct);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (Hub is null) return;
        await Hub.StopAsync(ct);
        await Hub.DisposeAsync();
    }
    
    public async Task JoinRoom(CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(JoinRoom), roomId, cancellationToken: ct);
    
    public async Task Start(TimeSpan interval, CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(Start), roomId, interval, cancellationToken: ct);
}