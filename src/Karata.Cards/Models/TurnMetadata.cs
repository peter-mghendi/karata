using Karata.Kit.Domain.Models;

namespace Karata.Cards.Models;

public record TurnMetadata
{
    public EngineData? Engine { get; init; }
    public TurnValidationProblem? Problem { get; set; }
}