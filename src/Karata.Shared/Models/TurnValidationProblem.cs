using System.Collections.Immutable;

namespace Karata.Shared.Models;

public record TurnValidationProblem
{
    public required int Index { get; init; }
    public required ImmutableArray<Card> Cards { get; init; }
    public required string Kind { get; init; }
    public required string Message { get; init; }
}