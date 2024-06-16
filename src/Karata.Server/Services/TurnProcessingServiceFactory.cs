using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class TurnProcessingServiceFactory(
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    KarataEngineFactory factory,
    ILoggerFactory loggers
)
{
    public TurnProcessingService Create(Room room, User user, string client)
    {
        var logger = loggers.CreateLogger<TurnProcessingService>();
        return new TurnProcessingService(hub, context, factory, logger, user, room, client);
    }
}