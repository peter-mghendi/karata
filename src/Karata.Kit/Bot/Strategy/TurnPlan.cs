using System.Collections.Immutable;
using Karata.Cards;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Strategy;

public sealed record TurnPlan(
    ImmutableArray<Card> Move,
    Func<bool, Card?> RequestFactory,
    Func<RoomData, bool> LastCardStatusFactory
);