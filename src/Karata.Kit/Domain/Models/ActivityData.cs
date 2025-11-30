namespace Karata.Kit.Domain.Models;

public record ActivityData
{
    public required ActivityType Type { get; init; }
    public required string Text { get; init; }
    public required string Action { get; init; }
    public required string Link { get; init; }
    public required Dictionary<string, object> Metadata { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public required UserData Actor { get; init; }
}