using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Cards.Services;

public class RoomMembershipServiceFactory(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    IPasswordService passwords,
    CardsContext context,
    PresenceService presence
)
{
    public RoomMembershipService Create(Guid room, string player) => 
        new(players, spectators, passwords, context, presence, room, player);
}