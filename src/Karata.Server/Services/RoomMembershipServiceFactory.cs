using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class RoomMembershipServiceFactory(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    IPasswordService passwords,
    KarataContext context,
    PresenceService presence
)
{
    public RoomMembershipService Create(Guid room, string player) => 
        new(players, spectators, passwords, context, presence, room, player);
}