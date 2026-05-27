using Karata.Kit.Application.Client.Connection;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Microsoft.Extensions.Logging;

namespace Karata.Kit.Bot.Services;

public sealed class BotSessionFactory(PlayerConnection connection, IKarataEngine engine, ILoggerFactory loggers)
{
    public BotSession Create(UserData player, IBotStrategy strategy) => new(player, strategy, connection, engine, loggers);
}