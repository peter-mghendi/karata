using System.Collections.Immutable;
using Karata.Pips;

namespace Karata.Kit.Cards.Engine.Exceptions;

public class InvalidCardSequenceException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(InvalidCardSequenceException), Narration)
{
    public const string Narration =
        "Invalid card sequence. Subsequent cards must either match the previous card's face or be answers.";
}