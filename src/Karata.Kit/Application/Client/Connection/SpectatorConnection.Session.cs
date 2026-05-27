using Karata.Kit.Application.Client.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Kit.Application.Client.Connection;


public sealed partial class SpectatorConnection
{
    public sealed record SessionParameters : IUserConnection.ISessionParameters;

    public sealed class Session(Guid roomId, HubConnection hub) : IUserConnection.ISession
    {
        public HubConnection Hub { get; } = hub;
        public RoomEvents Events { get; } = new();
    
        public async Task JoinRoom(CancellationToken ct = default) =>
            await Hub.SendAsync(nameof(JoinRoom), roomId, cancellationToken: ct);

        public void Dispose() => Events.Dispose();
    }
}