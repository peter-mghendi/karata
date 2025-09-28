namespace Karata.Shared.Models;

public record HandData
{
    public required long Id { get; init; }
    public required HandStatus Status { get; set; }
    public required UserData Player { get; set; }
    public List<Card> Cards { get; set; } = [];
}