using System.ComponentModel.DataAnnotations.Schema;
using Karata.Shared.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.CardRequestLevel;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Models;

public class Game
{
    public int Id { get; set; }

    public GameStatus Status { get; set; }
    public bool IsReversed { get; set; }

    public Card? Request { get; set; }

    public CardRequestLevel RequestLevel => Request switch
    {
        null => NoRequest,
        { Face: None } => SuitRequest,
        _ => CardRequest
    };

    public uint Give { get; set; }
    public uint Pick { get; set; }
    public int CurrentTurn { get; set; }

    public int NextTurn => IsReversed
        ? (CurrentTurn - 1 + Hands.Count) % Hands.Count
        : (CurrentTurn + 1) % Hands.Count;

    public Deck Deck { get; init; } = Deck.Standard;
    public Pile Pile { get; init; } = new();

    public GameResult? Result { get; set; }

    public Guid RoomId { get; set; }

    public List<Hand> Hands { get; set; } = [];

    [NotMapped] public Hand CurrentHand => Hands[CurrentTurn];

    public HashSet<Hand> HandsExceptPlayerId(string playerId) => Hands.Where(h => h.Player.Id != playerId).ToHashSet();

    public GameData ToData() => new()
    {
        IsForward = IsReversed, // TODO: WTF? Was I on drugs
        Status = Status,
        CurrentRequest = Request, // TODO: Clean this up as well
        Pick = Pick,
        CurrentTurn = CurrentTurn,
        DeckCount = Deck.Count, // TODO: And consider a real deck (Ace of Spades * Deck.Count)
        Pile = Pile,
        Hands = Hands.Select(h => h.ToData()).ToList(),
        Result = Result?.ToData(),
    };
}