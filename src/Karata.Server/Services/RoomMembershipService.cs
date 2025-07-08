using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public partial class RoomMembershipService (
    IHubContext<GameHub, IGameClient> hub,
    IPasswordService passwords,
    KarataContext context,
    PresenceService presence,
    TurnManagementService turns,
    UserManager<User> users,
    Guid room,
    string player) : HubAwareService(hub, room, player);