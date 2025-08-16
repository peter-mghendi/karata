using System.Collections.Immutable;

namespace Karata.Shared.Engine.Exceptions;

public class InvalidCardSequenceException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(InvalidCardSequenceException), Narration)
{
    public const string Narration =
        "Invalid card sequence. Subsequent cards must either match the previous card's face or be answers.";
}