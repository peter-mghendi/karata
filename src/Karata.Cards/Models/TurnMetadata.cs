using Karata.Kit.Domain.Models;

namespace Karata.Cards.Models;

public record TurnMetadata
{
    public TurnValidationProblem? Problem { get; set; }
}