using Karata.Cards;
using Karata.Kit.Application.Client.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Kit.Application.Client.Connection;


public sealed partial class PlayerConnection
{
    public sealed class Session(Guid roomId, HubConnection hub) : IUserConnection.ISession
    {
        public HubConnection Hub { get; } = hub;
        public RoomEvents Events { get; init; } = new();
        
        public async Task JoinRoom(CancellationToken ct = default) =>
            await Hub.SendAsync(nameof(JoinRoom), roomId, cancellationToken: ct);

        public async Task LeaveRoom(CancellationToken ct = default) =>
            await Hub.SendAsync(nameof(LeaveRoom), roomId, cancellationToken: ct);

        public async Task StartGame(CancellationToken ct = default) =>
            await Hub.SendAsync(nameof(StartGame), roomId, cancellationToken: ct);

        public Task PerformTurn(IReadOnlyList<Card> cards, CancellationToken ct = default)
            => Hub.SendAsync(nameof(PerformTurn), roomId, cards, cancellationToken: ct);

        public Task SendChat(string message, CancellationToken ct = default)
            => Hub.SendAsync(nameof(SendChat), roomId, message, cancellationToken: ct);

        public async Task SetAway(long handId, CancellationToken ct = default) =>
            await Hub.SendAsync(nameof(SetAway), roomId, handId, cancellationToken: ct);

        public async Task VoidTurn(long handId, CancellationToken ct = default) =>
            await Hub.SendAsync(nameof(VoidTurn), roomId, handId, cancellationToken: ct);

        public void Dispose()
        {
            Events.Dispose();
        }
    }
}