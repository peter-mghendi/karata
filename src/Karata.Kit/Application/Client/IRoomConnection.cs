using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Kit.Application.Client;

public interface IRoomConnection
{
    HubConnection? Hub { get; }
    RoomEvents Events { get; }
    Task StopAsync();
}