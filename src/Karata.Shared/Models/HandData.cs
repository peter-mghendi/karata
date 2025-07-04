namespace Karata.Shared.Models;

public record HandData
{
    public required UserData User { get; set; }
    public List<Card> Cards { get; set; } = [];
    // public List<TurnData> Turns { get; set; } = [];
}