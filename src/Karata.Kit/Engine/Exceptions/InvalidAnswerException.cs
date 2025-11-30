using System.Collections.Immutable;
using Karata.Cards;

namespace Karata.Kit.Engine.Exceptions;

public class InvalidAnswerException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(InvalidAnswerException), Narration)
{
    public const string Narration =
        "Invalid answer card. An answer is only valid if it is the same face or suit as the previous card.";
}