using System.Collections.Immutable;
using Karata.Kit.Cards.Models;
using Karata.Pips;

namespace Karata.Kit.Cards.Engine.Exceptions;

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