using Karata.Kit.Engine;
using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class TurnProcessingServiceFactory(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILoggerFactory loggers,
    KarataContext context,
    IKarataEngine engine
)
{
    public TurnProcessingService Create(Guid room, string player, string connection) =>
        new(
            players,
            spectators,
            loggers.CreateLogger<TurnProcessingService>(),
            context,
            engine,
            room,
            player,
            connection
        );
}