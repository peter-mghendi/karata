namespace Karata.Server.Models;

public record EngineData
{
    public required string Name { get; init; }
    public required string Date { get; init; }
    public required string Branch { get; init; }
    public required string Version { get; init; }
    public required string Revision { get; init; }
}