using Karata.Kit.Bot.Strategy;
using Karata.Kit.Cards.Connection;
using Karata.Kit.Cards.Engine;
using Karata.Kit.Cards.Models;
using Microsoft.Extensions.Logging;

namespace Karata.Kit.Bot.Services;

public sealed class BotSessionFactory(PlayerConnection connection, IKarataEngine engine, ILoggerFactory loggers)
{
    public BotSession Create(UserData player, IBotStrategy strategy) => new(player, strategy, connection, engine, loggers);
}