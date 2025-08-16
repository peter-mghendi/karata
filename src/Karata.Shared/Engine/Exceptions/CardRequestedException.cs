using System.Collections.Immutable;

namespace Karata.Shared.Engine.Exceptions;

public class CardRequestedException(int index, ImmutableArray<Card> cards) 
    : TurnValidationException(index, cards, nameof(CardRequestedException), Narration)
{
    public const string Narration =
        "A card has been requested. Your turn must either start with the requested card or block the request.";
        
}