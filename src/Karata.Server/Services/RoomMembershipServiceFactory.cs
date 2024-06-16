using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class RoomMembershipServiceFactory(
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    IPasswordService passwords,
    PresenceService presence
)
{
    public RoomMembershipService Create(Room room, User user, string client)
    {
        return new RoomMembershipService(hub, context, passwords, presence, user, room, client);
    }
}