using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Cards.Services;

public partial class RoomMembershipService(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    CardsContext context,
    PresenceService presence,
    Guid roomId,
    string player
) : LiveRoomAwareService(players, spectators, roomId, player);