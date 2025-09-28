using Karata.Cards;
using Karata.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Bot.Services;

public sealed class PlayerConnection(string url, string room, string? password, Func<Task<string?>> factory)
{
    private HubConnection? _hub;

    public RoomStreams Streams { get; } = new();

    public async Task StartAsync(Func<bool, Card?> request, CancellationToken ct = default)
    {
        _hub = new HubConnectionBuilder()
            .WithUrl($"{url}/hubs/game/play", o => o.AccessTokenProvider = factory)
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        _hub.On<RoomData>("AddToRoom", r => Streams.OnAddToRoom(r));
        _hub.On<long, UserData, HandStatus>("AddHandToRoom", (i, u,s) => Streams.OnAddHandToRoom(i, u,s));
        _hub.On<UserData, int>("MoveCardCountFromDeckToHand", (u,n) => Streams.OnMoveCardCountFromDeckToHand(u,n));
        _hub.On<List<Card>>("MoveCardsFromDeckToHand", c => Streams.OnMoveCardsFromDeckToHand(c));
        _hub.On<List<Card>>("MoveCardsFromDeckToPile", c => Streams.OnMoveCardsFromDeckToPile(c));
        _hub.On<UserData, List<Card>>("MoveCardsFromHandToPile", (u,c) => Streams.OnMoveCardsFromHandToPile(u,c));
        _hub.On("NotifyTurnProcessed", () => Streams.OnNotifyTurnProcessed());
        _hub.On<ChatData>("ReceiveChat", m => Streams.OnReceiveChat(m));
        _hub.On<SystemMessage>("ReceiveSystemMessage", m => Streams.OnReceiveSystemMessage(m));
        _hub.On("ReclaimPile", () => Streams.OnReclaimPile());
        _hub.On<UserData>("RemoveHandFromRoom", u => Streams.OnRemoveHandFromRoom(u));
        _hub.On<Card>("SetCurrentRequest", c => Streams.OnSetCurrentRequest(c));
        _hub.On<UserData>("UpdateAdministrator", a => Streams.OnUpdateAdministrator(a));
        _hub.On<GameStatus>("UpdateGameStatus", s => Streams.OnUpdateGameStatus(s));
        _hub.On<UserData, HandStatus>("UpdateHandStatus", (u,s) => Streams.OnUpdateHandStatus(u,s));
        _hub.On<uint>("UpdatePick", n => Streams.OnUpdatePick(n));
        _hub.On<int>("UpdateTurn", t => Streams.OnUpdateTurn(t));

        _hub.On<bool, Card?>("PromptCardRequest", request);
        _hub.On("PromptLastCardRequest", () => true);
        _hub.On("PromptPasscode", () => password);

        await _hub.StartAsync(ct);
        await _hub.SendAsync("JoinRoom", room, cancellationToken: ct);

        await SendChat("hi guys", ct);
    }

    public Task PerformTurn(IReadOnlyList<Card> cards, CancellationToken ct = default)
        => _hub!.SendAsync("PerformTurn", room, cards, cancellationToken: ct);

    public Task SendChat(string message, CancellationToken ct = default)
        => _hub!.SendAsync("SendChat", room, message, cancellationToken: ct);

    public async Task StopAsync()
    {
        if (_hub is null) return;
        await _hub.StopAsync();
        await _hub.DisposeAsync();
    }
}
