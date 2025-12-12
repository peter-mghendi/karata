using System.Collections.Immutable;
using Karata.Cards;
using Karata.Kit.Domain.Models;

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