using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public partial class RoomMembershipService (
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    IPasswordService passwords,
    PresenceService presence,
    User user,
    Room room,
    string client
) : HubAwareService(hub, room, client);