using Karata.Kit.Cards.Models;
using Karata.Kit.Platform.Models;
using OpenTelemetry.Trace;

namespace Karata.Platform.Models;

public class Activity
{
    public required Guid Id { get; set; }
    public required string Subject { get; set; }
    public required string Application { get; set; }
    public required string Text { get; set; }
    public required List<ActionData> Actions { get; set; }
    public required Dictionary<string, string> Metadata { get; set; }
    public required DateTimeOffset OccurredAt { get; set; }
    public required DateTimeOffset RecordedAt { get; set; }
}