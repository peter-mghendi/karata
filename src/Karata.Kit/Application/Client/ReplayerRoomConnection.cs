using Karata.Cards;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Application.Client;

public sealed class ReplayerRoomConnection(Uri host, Guid room) : IRoomConnection
{
    public HubConnection? Hub { get; private set; }
    public RoomEvents Events { get; } = new();
    public required Func<Task<string?>> AccessTokenProvider { get; init; }

    public async Task StartAsync(CancellationToken cancellation = default)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(host, "/hubs/game/replay"), o => o.AccessTokenProvider = AccessTokenProvider)
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        Hub.On<RoomData>("AddToRoom", r => Events.OnAddToRoom(r));
        Hub.On<long, UserData, HandStatus>("AddHandToRoom", (i, u, s) => Events.OnAddHandToRoom(i, u, s));
        Hub.On("EndGame", () => Events.OnEndGame());
        Hub.On<UserData, List<Card>>("MoveCardsFromDeckToHand", (u, c) => Events.OnMoveCardsFromDeckToHand(u, c));
        Hub.On<List<Card>>("MoveCardsFromDeckToPile", c => Events.OnMoveCardsFromDeckToPile(c));
        Hub.On<UserData, List<Card>, bool>("MoveCardsFromHandToPile", (u, c, v) => Events.OnMoveCardsFromHandToPile(u, c, v));
        Hub.On<SystemMessage>("ReceiveSystemMessage", m => Events.OnReceiveSystemMessage(m));
        Hub.On("ReclaimPile", () => Events.OnReclaimPile());
        Hub.On("RemoveFromRoom", () => Events.OnRemoveFromRoom());
        Hub.On<UserData>("RemoveHandFromRoom", u => Events.OnRemoveHandFromRoom(u));
        Hub.On<Card>("SetCurrentRequest", c => Events.OnSetCurrentRequest(c));
        Hub.On<UserData>("UpdateAdministrator", a => Events.OnUpdateAdministrator(a));
        Hub.On<GameStatus>("UpdateGameStatus", s => Events.OnUpdateGameStatus(s));
        Hub.On<UserData, HandStatus>("UpdateHandStatus", (u, s) => Events.OnUpdateHandStatus(u, s));
        Hub.On<uint>("UpdatePick", n => Events.OnUpdatePick(n));
        Hub.On<int>("UpdateTurn", t => Events.OnUpdateTurn(t));
        
        await Hub.StartAsync(cancellation);
        await Hub.SendAsync("JoinRoom", room, cancellationToken: cancellation);
    }

    public async Task StopAsync()
    {
        if (Hub is null) return;
        await Hub.StopAsync();
        await Hub.DisposeAsync();
    }
}