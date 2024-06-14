namespace Karata.Shared.Models;

public record HandData
{
    public List<Card> Cards { get; set; } = [];
    public required UserData User { get; set; }
}