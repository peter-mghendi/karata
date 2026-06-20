using System.Collections.Immutable;
using Karata.Kit.Domain.Models;
using Karata.Pips;

namespace Karata.Kit.Engine.Exceptions;

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