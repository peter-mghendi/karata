using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class TurnProcessingServiceFactory(
    IHubContext<PlayerHub, IPlayerClient> players,
    IHubContext<SpectatorHub, ISpectatorClient> spectators,
    ILoggerFactory loggers,
    KarataContext context,
    UserManager<User> users
)
{
    public TurnProcessingService Create(Guid room, string player, string connection) =>
        new TurnProcessingService(players,
            spectators,
            loggers.CreateLogger<TurnProcessingService>(),
            context,
            users,
            room,
            player,
            connection
        );
}