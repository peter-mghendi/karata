using Karata.Kit.Application.Client;
using Karata.Kit.Bot.Infrastructure.Security;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Karata.Kit.Bot.Services;

public sealed class BotSessionFactory(IServiceProvider services, IConfiguration config, AccessTokenProvider tokens)
{
    public BotSession Create(IBotStrategy strategy, UserData player, Guid roomId, string? password)
    {
        var url = new Uri(config["Karata:Host"]!);
        var subscriber = new PlayerRoomConnection(url, roomId)
        {
            AccessTokenProvider = async () => await tokens.GetAccessTokenAsync(),
            RoomPasswordProvider = () => Task.FromResult(password)
        };

        var engine = services.GetRequiredService<IKarataEngine>();
        var loggers = services.GetRequiredService<ILoggerFactory>();
        return new BotSession(player, subscriber, strategy, engine, loggers);
    }
}