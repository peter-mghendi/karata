using Karata.Cards;
using Karata.Kit.Application.Client.State;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Application.Client.Connection;

public sealed class SpectatorRoomConnection(Uri host, Guid roomId) : IRoomConnection<SpectatorRoomConnection.StartParameters>
{
    public sealed record StartParameters : IRoomConnectionStartParameters;
    
    private const string HubPath = "/hubs/game/spectate";
    
    public HubConnection? Hub { get; private set; }
    public RoomEvents Events { get; } = new();

    public async Task StartAsync(StartParameters _, CancellationToken ct = default)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(host, HubPath))
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        Hub.On<RoomData>(nameof(Events.AddToRoom), room => Events.OnAddToRoom(room));
        Hub.On<long, UserData, HandStatus>(nameof(Events.AddHandToRoom), (handId, user, status) => Events.OnAddHandToRoom(handId, user, status));
        Hub.On(nameof(Events.EndGame), () => Events.OnEndGame());
        Hub.On<long, List<Card>>(nameof(Events.MoveCardsFromDeckToHand), (handId, cards) => Events.OnMoveCardsFromDeckToHand(handId, cards));
        Hub.On<List<Card>>(nameof(Events.MoveCardsFromDeckToPile), cards => Events.OnMoveCardsFromDeckToPile(cards));
        Hub.On<long, List<Card>, bool>(nameof(Events.MoveCardsFromHandToPile), (handId, cards, visible) => Events.OnMoveCardsFromHandToPile(handId, cards, visible));
        Hub.On<SystemMessage>(nameof(Events.ReceiveSystemMessage), m => Events.OnReceiveSystemMessage(m));
        Hub.On(nameof(Events.ReclaimPile), () => Events.OnReclaimPile());
        Hub.On(nameof(Events.RemoveFromRoom), () => Events.OnRemoveFromRoom());
        Hub.On<long>(nameof(Events.RemoveHandFromRoom), handId => Events.OnRemoveHandFromRoom(handId));
        Hub.On<Card>(nameof(Events.SetCurrentRequest), cards => Events.OnSetCurrentRequest(cards));
        Hub.On<UserData>(nameof(Events.UpdateAdministrator), user => Events.OnUpdateAdministrator(user));
        Hub.On<GameStatus>(nameof(Events.UpdateGameStatus), status => Events.OnUpdateGameStatus(status));
        Hub.On<long, HandStatus>(nameof(Events.UpdateHandStatus), (handId, status) => Events.OnUpdateHandStatus(handId, status));
        Hub.On<uint>(nameof(Events.UpdatePick), n => Events.OnUpdatePick(n));
        Hub.On<int>(nameof(Events.UpdateTurn), t => Events.OnUpdateTurn(t));
        
        await Hub.StartAsync(ct);
        await JoinRoom(ct);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        if (Hub is null) return;
        await Hub.StopAsync(ct);
        await Hub.DisposeAsync();
    }
    
    public async Task JoinRoom(CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(JoinRoom), roomId, cancellationToken: ct);
}