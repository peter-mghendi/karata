namespace Karata.Shared.Models;

public record GameData
{
    public bool IsReversed { get; set; } = true;
    public GameStatus Status { get; set; }
    public Card? Request { get; set; }
    public uint Pick { get; set; }
    public int CurrentTurn { get; set; }
    public int DeckCount { get; set; } = Deck.Standard.Count;
    public Pile Pile { get; set; } = new();
    public List<HandData> Hands { get; set; } = [];
    public GameResultData? Result { get; set; }

    public HandData CurrentHand => Hands[CurrentTurn];
}