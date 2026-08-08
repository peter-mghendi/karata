using Karata.Kit.Cards.Models;

namespace Karata.Kit.Platform.Models;

public record ActivityData
{
    public required Guid Id { get; init; }
    public required UserData Actor { get; init; }
    public required ApplicationData Application { get; init; }
    public required string Text { get; init; }
    public required List<ActionData> Actions { get; set; }
    public required Dictionary<string, string> Metadata { get; set; }
    public DateTimeOffset OccurredAt { get; init; }
}