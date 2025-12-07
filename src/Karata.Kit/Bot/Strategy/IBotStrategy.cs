using System.Collections.Immutable;
using Karata.Cards;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Strategy;

public interface IBotStrategy
{
    string Name { get; }

    string Description { get; }

    ImmutableArray<Card> Decide(RoomData room, List<Card> cards);

    Card? Request(RoomData room, List<Card> cards, bool specific);

    BotData Data => new(Name: Name, Description: Description);
}