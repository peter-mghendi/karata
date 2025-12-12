using System.Collections.Immutable;
using Karata.Cards;

namespace Karata.Kit.Engine.Exceptions;

public class InvalidFirstCardException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(InvalidFirstCardException), Narration)
{
    public const string Narration =
        "Invalid first card. The first card played must match the face or suit of the previous card.";
}