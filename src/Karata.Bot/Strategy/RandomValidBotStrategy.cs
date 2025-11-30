using System.Collections.Immutable;
using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Domain.Models;
using Karata.Kit.Engine;
using Karata.Kit.Engine.Exceptions;
using static Karata.Cards.Card.CardFace;

namespace Karata.Bot.Strategy;

public class RandomValidBotStrategy(IKarataEngine engine) : IBotStrategy
{
    private static readonly Random Random = new();

    public ImmutableArray<Card> Decide(RoomData room, List<Card> cards) =>
        cards
            .OrderBy(_ => Random.Next())
            .FirstOrDefault(candidate => TryCard(room, candidate)) is {} card ? [card] : [];

    public Card? Request(RoomData room, List<Card> cards, bool specific)
    {
        var decision = Decide(room, cards);
        var candidate = cards.Except(decision).FirstOrDefault();
        if (candidate is null) return null;

        return specific switch
        {
            true => candidate,
            false => None.Of(candidate.Suit),
        };
    }

    private bool TryCard(RoomData room, Card card)
    {
        try
        {
            _ = engine.EvaluateTurn(room.Game, [card]);
            return true;
        }
        catch (KarataEngineException)
        {
            return false;
        }
    }
}