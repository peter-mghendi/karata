using Karata.Bot.Infrastructure.Security;
using Karata.Bot.Strategy;
using Karata.Kit.Application.Client;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;

namespace Karata.Bot.Services;

public sealed class BotSessionFactory(
    IServiceProvider services,
    IConfiguration configuration,
    AccessTokenProvider tokens
)
{
    private static readonly string[] Strategies = ["bail", "random-valid"];
    private static readonly Random Random = new();
    private static string RandomStrategy => Strategies.OrderBy(_ => Random.Next()).First();

    public BotSession Create(UserData player, Guid roomId, string? password)
    {
        var url = new Uri(configuration["Karata:Host"]!);
        var subscriber = new PlayerRoomConnection(url, roomId)
        {
            AccessTokenProvider = async () => await tokens.GetAccessTokenAsync(),
            RoomPasswordProvider = () => Task.FromResult(password)
        };

        IBotStrategy strategy = RandomStrategy switch
        {
            "bail" => ActivatorUtilities.CreateInstance<BailBotStrategy>(services),
            "random-valid" => ActivatorUtilities.CreateInstance<RandomValidBotStrategy>(services),
            _ => ActivatorUtilities.CreateInstance<BailBotStrategy>(services)
        };

        var engine = services.GetRequiredService<IKarataEngine>();
        var loggers = services.GetRequiredService<ILoggerFactory>();
        return new BotSession(player, subscriber, strategy, engine, loggers);
    }
}