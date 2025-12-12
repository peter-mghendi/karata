using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public partial class RoomMembershipService (
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    IPasswordService passwords,
    KarataContext context,
    PresenceService presence,
    Guid room,
    string player) : RoomAwareService(players, spectators, room, player);