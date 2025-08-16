using System.Collections.Immutable;
using Karata.Shared.Models;

namespace Karata.Shared.Engine.Exceptions;

public abstract class TurnValidationException(
    int index,
    ImmutableArray<Card> cards,
    string kind,
    string message
) : KarataEngineException
{
    public override string Message => message;
    public TurnValidationProblem Problem { get; } = new()
    {
        Index = index, 
        Cards = cards, 
        Kind = kind, 
        Message = message
    };
}