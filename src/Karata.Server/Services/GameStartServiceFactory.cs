using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

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