using Karata.Kit.Application.Client.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Kit.Application.Client.Connection;

public interface IRoomConnection {
    HubConnection? Hub { get; }
    RoomEvents Events { get; }
    Task StopAsync(CancellationToken ct = default);
}

public interface IRoomConnection<in T> : IRoomConnection where T : IRoomConnectionStartParameters {
    Task StartAsync(T parameters, CancellationToken ct = default);
}