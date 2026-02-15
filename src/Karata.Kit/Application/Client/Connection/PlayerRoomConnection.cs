using Karata.Cards;
using Karata.Kit.Application.Client.State;
using Karata.Kit.Domain.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Application.Client.Connection;

public sealed class PlayerRoomConnection(Uri host, Guid roomId) : IRoomConnection<PlayerRoomConnection.StartParameters>
{
    public sealed record StartParameters(
        Func<bool, Task<Card?>> OnRequestCard,
        Func<Task<bool>> OnRequestLastCard
    ) : IRoomConnectionStartParameters;

    private const string HubPath = "/hubs/game/play";

    public HubConnection? Hub { get; private set; }
    public RoomEvents Events { get; } = new();
    public required Func<Task<string?>> AccessTokenProvider { get; init; }
    public required Func<Task<string?>> RoomPasswordProvider { get; init; }

    public async Task StartAsync(StartParameters parameters, CancellationToken ct = default)
    {
        Hub = new HubConnectionBuilder()
            .WithUrl(new Uri(host, HubPath), o => o.AccessTokenProvider = AccessTokenProvider)
            .WithAutomaticReconnect()
            .AddJsonProtocol()
            .Build();

        Hub.On<RoomData>(nameof(Events.AddToRoom), room => Events.OnAddToRoom(room));
        Hub.On<long, UserData, HandStatus>(nameof(Events.AddHandToRoom),
            (handId, user, status) => Events.OnAddHandToRoom(handId, user, status));
        Hub.On(nameof(Events.EndGame), () => Events.OnEndGame());
        Hub.On<long, IReadOnlyList<Card>>(nameof(Events.MoveCardsFromDeckToHand),
            (handId, cards) => Events.OnMoveCardsFromDeckToHand(handId, cards));
        Hub.On<List<Card>>(nameof(Events.MoveCardsFromDeckToPile), c => Events.OnMoveCardsFromDeckToPile(c));
        Hub.On<long, List<Card>, bool>(nameof(Events.MoveCardsFromHandToPile),
            (handId, cards, visible) => Events.OnMoveCardsFromHandToPile(handId, cards, visible));
        Hub.On(nameof(Events.NotifyTurnProcessed), () => Events.OnNotifyTurnProcessed());
        Hub.On<ChatData>(nameof(Events.ReceiveChat), chat => Events.OnReceiveChat(chat));
        Hub.On<SystemMessage>(nameof(Events.ReceiveSystemMessage), message => Events.OnReceiveSystemMessage(message));
        Hub.On(nameof(Events.ReclaimPile), () => Events.OnReclaimPile());
        Hub.On(nameof(Events.RemoveFromRoom), () => Events.OnRemoveFromRoom());
        Hub.On<long>(nameof(Events.RemoveHandFromRoom), handId => Events.OnRemoveHandFromRoom(handId));
        Hub.On<Card>(nameof(Events.SetCurrentRequest), card => Events.OnSetCurrentRequest(card));
        Hub.On<UserData>(nameof(Events.UpdateAdministrator), user => Events.OnUpdateAdministrator(user));
        Hub.On<GameStatus>(nameof(Events.UpdateGameStatus), status => Events.OnUpdateGameStatus(status));
        Hub.On<long, HandStatus>(nameof(Events.UpdateHandStatus),
            (handId, status) => Events.OnUpdateHandStatus(handId, status));
        Hub.On<uint>(nameof(Events.UpdatePick), pick => Events.OnUpdatePick(pick));
        Hub.On<int>(nameof(Events.UpdateTurn), turn => Events.OnUpdateTurn(turn));

        Hub.On<string?>("PromptPasscode", RoomPasswordProvider);
        Hub.On<bool, Card?>("PromptCardRequest", parameters.OnRequestCard);
        Hub.On<bool>("PromptLastCardRequest", parameters.OnRequestLastCard);

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

    public async Task LeaveRoom(CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(LeaveRoom), roomId, cancellationToken: ct);

    public async Task StartGame(CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(StartGame), roomId, cancellationToken: ct);

    public Task PerformTurn(IReadOnlyList<Card> cards, CancellationToken ct = default)
        => Hub!.SendAsync(nameof(PerformTurn), roomId, cards, cancellationToken: ct);

    public Task SendChat(string message, CancellationToken ct = default)
        => Hub!.SendAsync(nameof(SendChat), roomId, message, cancellationToken: ct);

    public async Task SetAway(long handId, CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(SetAway), roomId, handId, cancellationToken: ct);

    public async Task VoidTurn(long handId, CancellationToken ct = default) =>
        await Hub!.SendAsync(nameof(VoidTurn), roomId, handId, cancellationToken: ct);
}