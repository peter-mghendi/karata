using Karata.Shared.Models;

namespace Karata.Server.Models;

public record TurnMetadata
{
    public EngineData? Engine { get; init; }
    public TurnValidationProblem? Problem { get; init; }
}