using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class GameStartServiceFactory(
    IHubContext<GameHub, IGameClient> hub,
    KarataContext context,
    ILoggerFactory loggers
)
{
    public GameStartService Create(Room room, User user, string client)
    {
        var logger = loggers.CreateLogger<GameStartService>();
        return new GameStartService(hub, context, logger, user, room, client);
    }
}