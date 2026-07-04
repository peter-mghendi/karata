namespace Karata.Kit.Platform.Models;

public class ActivityRequest
{
    public required string Text { get; set; }
    public required List<ActionData> Actions { get; set; }
    public required Dictionary<string, string> Metadata { get; set; }
    public required DateTimeOffset OccurredAt { get; set; }
}