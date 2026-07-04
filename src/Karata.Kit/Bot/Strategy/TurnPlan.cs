using System.Collections.Immutable;
using Karata.Kit.Cards.Models;
using Karata.Pips;

namespace Karata.Kit.Bot.Strategy;

public sealed record TurnPlan(
    ImmutableArray<Card> Move,
    Func<bool, Card?> RequestFactory,
    Func<RoomData, bool> LastCardStatusFactory
);