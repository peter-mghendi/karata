using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

/// <summary>
/// Base for services that need SignalR context and room/user identification.
/// Provides helper methods to broadcast to specific subsets of clients.
/// </summary>
public abstract class HubAwareService(IHubContext<GameHub, IGameClient> hub, Guid room, string player)
{
    protected readonly Guid RoomId = room;
    protected readonly string CurrentPlayerId = player;
    // protected readonly string ConnectionId = connection;

    protected IGameClient Me => hub.Clients.User(CurrentPlayerId);
    protected IGameClient Room => hub.Clients.Group(RoomId.ToString());
    protected IGameClient Client(string connection) => hub.Clients.Client(connection);
    protected IGameClient Hand(Hand hand) => hub.Clients.User(hand.Player.Id);
    protected IGameClient Hands(HashSet<Hand> hands) => hub.Clients.Users(hands.Select(h => h.Player.Id).ToList());

    protected async Task AddToRoom(string connection) =>
        await hub.Groups.AddToGroupAsync(connection, RoomId.ToString());

    protected async Task RemoveFromRoom(string connection) =>
        await hub.Groups.RemoveFromGroupAsync(connection, RoomId.ToString());
}