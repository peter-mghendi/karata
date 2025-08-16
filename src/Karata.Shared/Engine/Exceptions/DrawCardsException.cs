using System.Collections.Immutable;

namespace Karata.Shared.Engine.Exceptions;

public class DrawCardsException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(DrawCardsException), Narration)
{
    public const string Narration =
        "You have pending cards to draw. You must either draw these cards, add to them or block them.";
}