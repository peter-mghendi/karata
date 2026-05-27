using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Strategy;

public sealed class BailBotStrategy : IBotStrategy
{
    public string Name => "Bail bot";

    public string Description => "A bot that always passes on its turn.";

    public Task<TurnPlan> PlanAsync(RoomData _, CancellationToken ct = default) =>
        Task.FromResult(new TurnPlan(Move: [], RequestFactory: _ => null, LastCardStatusFactory: _ => true));
}