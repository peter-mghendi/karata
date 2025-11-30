using System.Collections.Immutable;
using Karata.Cards;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Strategy;

public interface IBotStrategy
{
    ImmutableArray<Card> Decide(RoomData room, List<Card> cards);
    
    Card? Request(RoomData room, List<Card> cards, bool specific);
}