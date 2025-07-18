using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class TurnProcessingServiceFactory(
    IHubContext<GameHub, IGameClient> hub,
    ILoggerFactory loggers,
    KarataContext context,
    TurnManager turns,
    UserManager<User> users
)
{
    public TurnProcessingService Create(Guid room, string player, string connection)
    {
        var logger = loggers.CreateLogger<TurnProcessingService>();
        return new TurnProcessingService(hub, logger, context, turns, users, room, player, connection);
    }
}