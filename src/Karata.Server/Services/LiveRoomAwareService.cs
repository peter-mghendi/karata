using Karata.Kit.Domain.Models;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

/// <summary>
/// Base for services that need SignalR context and live room/user identification.
/// Provides helper methods to broadcast to specific subsets of clients.
/// </summary>
public abstract class LiveRoomAwareService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    Guid room,
    string player
)
{
    protected readonly Guid RoomId = room;
    protected readonly string CallerPlayerId = player;

    protected IPlayerClient Me => players.Clients.User(CallerPlayerId);
    protected IPlayerClient RoomPlayers => players.Clients.Group(RoomId.ToString());
    protected ISpectatorClient RoomSpectators => spectators.Clients.Group(RoomId.ToString());

    protected IPlayerClient Hand(Hand hand) => players.Clients.User(hand.Player.Id);
    protected IPlayerClient Hands(HashSet<Hand> hands) => players.Clients.Users([..hands.Select(h => h.Player.Id)]);
    protected IPlayerClient PlayerConnection(string connection) => players.Clients.Client(connection);
    protected async Task AddToRoom(string connection) =>
        await players.Groups.AddToGroupAsync(connection, RoomId.ToString());

    protected async Task RemoveFromRoom(string connection) =>
        await players.Groups.RemoveFromGroupAsync(connection, RoomId.ToString());
}
