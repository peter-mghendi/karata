namespace Karata.Shared.Models;

public record GameData
{
    public bool IsForward { get; set; } = true;
    public bool IsStarted { get; set; }
    public Card? CurrentRequest { get; set; }
    public int CurrentTurn { get; set; }
    public int DeckCount { get; set; } = Deck.Standard.Count;
    public Pile Pile { get; set; } = new();
    public List<HandData> Hands { get; set; } = [];
    public string? EndReason { get; set; }
}