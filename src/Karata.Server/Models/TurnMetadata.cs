using Karata.Kit.Domain.Models;

namespace Karata.Server.Models;

public record TurnMetadata
{
    public EngineData? Engine { get; init; }
    public TurnValidationProblem? Problem { get; init; }
}