using Karata.Kit.Bot.Models;
using Karata.Kit.Cards.Models;

namespace Karata.Kit.Bot.Strategy;

public interface IBotStrategy
{
    string Name { get; }

    string Description { get; }
    
    Task<TurnPlan> PlanAsync(RoomData room, CancellationToken ct = default);

    BotData Data => new(Name: Name, Description: Description);
}