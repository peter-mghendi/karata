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

    public Deck Deck { get; set; } = Deck.Standard;
    public Pile Pile { get; set; } = new();
    public string? EndReason { get; set; }

    public User? Winner { get; set; }
    public List<Hand> Hands { get; set; } = new();
    public List<Turn> Turns { get; set; } = new();

    public Guid RoomId { get; set; }

    public GameData ToData() => new()
    {
        IsForward = IsForward,
        IsStarted = IsStarted,
        CurrentRequest = CurrentRequest,
        CurrentTurn = CurrentTurn,
        DeckCount = Deck.Count,
        Pile = Pile,
        Hands = Hands.Select(h => h.ToData()).ToList(),
        EndReason = EndReason,
    };

    public GameRequestLevel RequestLevel => CurrentRequest switch
        {
            null => NoRequest,
            { Face: None } => SuitRequest,
            _ => CardRequest
        };
}