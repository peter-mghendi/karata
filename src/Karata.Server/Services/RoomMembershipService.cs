using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public partial class RoomMembershipService (
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    IPasswordService passwords,
    KarataContext context,
    PresenceService presence,
    UserManager<User> users,
    Guid room,
    string player) : HubAwareService(players, spectators, room, player);