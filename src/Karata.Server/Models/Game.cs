using Karata.Shared.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.GameRequestLevel;

namespace Karata.Server.Models;

public class Game
{
    public int Id { get; set; }

    // TODO: Refactor this into "Reversed"
    public bool IsForward { get; set; } = true;
    public bool IsStarted { get; set; }

    public Card? CurrentRequest { get; set; }
    public uint Give { get; set; }
    public uint Pick { get; set; }
    public int CurrentTurn { get; set; }

    public Deck Deck { get; set; } = Deck.StandardDeck;
    public Pile Pile { get; set; } = new();

    public virtual User? Winner { get; set; }
    public virtual List<Hand> Hands { get; set; } = new();
    public virtual List<Turn> Turns { get; set; } = new();

    public Guid RoomId { get; set; }

    public UIGame ToUI() => new()
    {
        IsForward = IsForward,
        IsStarted = IsStarted,
        CurrentRequest = CurrentRequest,
        CurrentTurn = CurrentTurn,
        DeckCount = Deck.Count,
        Pile = Pile,
        Hands = Hands.Select(h => h.ToUI()).ToList(),
    };

    public GameRequestLevel RequestLevel => CurrentRequest switch
        {
            null => NoRequest,
            { Face: None } => SuitRequest,
            _ => CardRequest
        };
}