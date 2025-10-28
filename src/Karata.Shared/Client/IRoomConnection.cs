using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Shared.Client;

public interface IRoomConnection
{
    HubConnection? Hub { get; }
    RoomEvents Events { get; }
    Task StopAsync();
}