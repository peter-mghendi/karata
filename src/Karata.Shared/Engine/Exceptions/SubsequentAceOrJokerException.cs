using System.Collections.Immutable;

namespace Karata.Shared.Engine.Exceptions;

public class SubsequentAceOrJokerException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(SubsequentAceOrJokerException), Narration)
{
    public const string Narration = "Invalid card order. An subsequent Ace/Joker can only go on top of a question or another Ace/Joker.";
}