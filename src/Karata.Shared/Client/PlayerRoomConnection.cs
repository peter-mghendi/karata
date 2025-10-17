using Karata.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Shared.Client;

public sealed class PlayerRoomConnection(Uri url, Guid room) : IRoomConnection
{
    public HubConnection? Hub { get; private set; }
    public RoomEvents Events { get; } = new();
    public required Func<Task<string?>> AccessTokenProvider { get; init; }
    public required Func<Task<string?>> RoomPasswordProvider { get; init; }

    public async Task StartAsync(
        Func<bool, Task<Card?>> onRequestCard,
        Func<Task<bool>> onRequestLastCard,
        CancellationToken cancellation = default
    )
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(url, "/hubs/game/play"), o => o.AccessTokenProvider = AccessTokenProvider)
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        Hub.On<RoomData>("AddToRoom", r => Events.OnAddToRoom(r));
        Hub.On<long, UserData, HandStatus>("AddHandToRoom", (i, u, s) => Events.OnAddHandToRoom(i, u, s));
        Hub.On("EndGame", () => Events.OnEndGame());
        Hub.On<UserData, IReadOnlyList<Card>>("MoveCardsFromDeckToHand", (u, c) => Events.OnMoveCardsFromDeckToHand(u, c));
        Hub.On<List<Card>>("MoveCardsFromDeckToPile", c => Events.OnMoveCardsFromDeckToPile(c));
        Hub.On<UserData, List<Card>, bool>("MoveCardsFromHandToPile", (u, c, v) => Events.OnMoveCardsFromHandToPile(u, c, v));
        Hub.On("NotifyTurnProcessed", () => Events.OnNotifyTurnProcessed());
        Hub.On<ChatData>("ReceiveChat", m => Events.OnReceiveChat(m));
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

        Hub.On<string?>("PromptPasscode", RoomPasswordProvider);
        Hub.On<bool, Card?>("PromptCardRequest", onRequestCard);
        Hub.On<bool>("PromptLastCardRequest", onRequestLastCard);

        await Hub.StartAsync(cancellation);
        await Hub.SendAsync("JoinRoom", room, cancellationToken: cancellation);

        await SendChat("hi guys", cancellation);
    }

    public async Task LeaveRoom() => await Hub!.SendAsync(nameof(LeaveRoom), room);

    public async Task StartGame() => await Hub!.SendAsync(nameof(StartGame), room);

    public Task PerformTurn(IReadOnlyList<Card> cards, CancellationToken ct = default)
        => Hub!.SendAsync(nameof(PerformTurn), room, cards, cancellationToken: ct);

    public Task SendChat(string message, CancellationToken ct = default)
        => Hub!.SendAsync(nameof(SendChat), room, message, cancellationToken: ct);

    public async Task SetAway(string playerId, CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(SetAway), room, playerId, cancellationToken: ct);

    public async Task VoidTurn(string playerId, CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(VoidTurn), room, playerId, cancellationToken: ct);

    public async Task StopAsync()
    {
        if (Hub is null) return;
        await Hub.StopAsync();
        await Hub.DisposeAsync();
    }
}