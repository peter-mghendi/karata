#nullable enable

using Karata.Web.Models.UI;
using static Karata.Cards.Card.CardFace;
using static Karata.Web.Models.GameRequestLevel;

namespace Karata.Web.Models;

public class Game
{
    public int Id { get; set; }

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

    public int RoomId { get; set; }

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