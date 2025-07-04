using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class RoomMembershipServiceFactory(
    IHubContext<GameHub, IGameClient> hub,
    IPasswordService passwords,
    KarataContext context,
    PresenceService presence,
    UserManager<User> users
)
{
    public RoomMembershipService Create(Guid room, string player, string connection)
    {
        return new RoomMembershipService(hub, passwords, context, presence, users, room, player, connection);
    }
}