using System.Collections.Immutable;
using Karata.Cards;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Strategy;

public class BailBotStrategy : IBotStrategy
{
    public string Name => "Bail bot";

    public string Description => "A bot that always passes on its turn.";
    
    public ImmutableArray<Card> Decide(RoomData room, List<Card> cards) => [];
    
    public Card? Request(RoomData room, List<Card> cards, bool specific) => null;
}