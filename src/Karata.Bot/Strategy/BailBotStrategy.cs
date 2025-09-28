using System.Collections.Immutable;
using Karata.Cards;
using Karata.Shared.Models;

namespace Karata.Bot.Strategy;

public class BailBotStrategy : IBotStrategy
{
    public ImmutableArray<Card> Decide(RoomData room, List<Card> cards) => [];
    public Card? Request(RoomData room, List<Card> cards, bool specific) => null;
}