using Karata.Cards.Data;
using Karata.Cards.Hubs;
using Karata.Cards.Hubs.Clients;
using Karata.Kit.Engine;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Cards.Services;

public class TurnProcessingServiceFactory(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILoggerFactory loggers,
    KarataContext context,
    IKarataEngine engine
)
{
    public LiveTurnProcessingService Create(Guid room, string player, string connection) =>
        new(
            players,
            spectators,
            loggers.CreateLogger<LiveTurnProcessingService>(),
            context,
            engine,
            room,
            player,
            connection
        );
}