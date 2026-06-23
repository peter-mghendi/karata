using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Cards.Services;

public class GameStartServiceFactory(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILoggerFactory loggers,
    KarataContext context
)
{
    public GameStartService Create(Guid room, string player) => 
        new GameStartService(players, spectators, loggers.CreateLogger<GameStartService>(), context, room, player);
}