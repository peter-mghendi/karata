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
    TurnManagementService turns,
    UserManager<User> users
)
{
    public RoomMembershipService Create(Guid room, string player)
    {
        return new RoomMembershipService(hub, passwords, context, presence, turns, users, room, player);
    }
}