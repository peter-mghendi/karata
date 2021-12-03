#nullable enable

namespace Karata.Web.Models.UI;

public class UIGame
{
    public bool IsForward { get; set; } = true;
    public bool IsStarted { get; set; } = false;
    public Card? CurrentRequest { get; set; } = null;
    public int CurrentTurn { get; set; } = 0;
    public int DeckCount { get; set; } = Deck.StandardDeck.Count;
    public Pile Pile { get; set; } = new();
    public List<UIHand> Hands { get; set; } = new();
}