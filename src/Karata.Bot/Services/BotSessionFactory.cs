using Karata.Bot.Infrastructure.Security;
using Karata.Bot.Strategy;
using Karata.Shared.Engine;
using Karata.Shared.Models;

namespace Karata.Bot.Services;

public sealed class BotSessionFactory(
    IServiceProvider services,
    IConfiguration configuration,
    KeycloakAccessTokenProvider tokens
)
{
    private static readonly string[] Strategies = ["bail", "random-valid"];
    private static readonly Random Random = new();
    private static string RandomStrategy => Strategies.OrderBy(_ => Random.Next()).First();

    public BotSession Create(UserData player, string roomId, string? password)
    {
        var url = configuration["Karata:Host"]!;
        var subscriber = new PlayerConnection(
            url,
            roomId,
            password,
            factory: async () => $"{await tokens.GetAccessTokenAsync()}"
        );

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