using Karata.Kit.Cards.Models;

namespace Karata.Cards.Models;

public record TurnMetadata
{
    public TurnValidationProblem? Problem { get; set; }
}