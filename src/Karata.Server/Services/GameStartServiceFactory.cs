using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class GameStartServiceFactory(
    IHubContext<PlayGameHub, IPlayGameClient> hub,
    ILoggerFactory loggers,
    KarataContext context
)
{
    public GameStartService Create(Guid room, string player)
    {
        var logger = loggers.CreateLogger<GameStartService>();
        return new GameStartService(hub, logger, context, room, player);
    }
}