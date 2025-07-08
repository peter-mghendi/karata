namespace Karata.Shared.Models;

public record HandData
{
    public required HandStatus Status { get; set; }
    public required UserData User { get; set; }
    public List<Card> Cards { get; set; } = [];
}