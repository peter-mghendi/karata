using System.ComponentModel.DataAnnotations.Schema;
using Karata.Shared.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Shared.Models.CardRequestLevel;

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
        { Face: not None } => CardRequest
    };

    public uint Give { get; set; }
    public uint Pick { get; set; }
    public int CurrentTurn { get; set; }

    public int NextTurn => IsReversed ? (CurrentTurn - 1 + Hands.Count) % Hands.Count : (CurrentTurn + 1) % Hands.Count;

    public Deck Deck { get; init; } = Deck.Standard;
    public Pile Pile { get; init; } = new();

    public GameResult? Result { get; set; }

    public Guid RoomId { get; set; }

    public List<Hand> Hands { get; set; } = [];

    [NotMapped] public Hand CurrentHand => Hands[CurrentTurn];

    public HashSet<Hand> HandsExceptPlayerId(string playerId) => Hands.Where(h => h.Player.Id != playerId).ToHashSet();

    private GameData ToData() => new()
    {
        IsReversed = IsReversed,
        Status = Status,
        Request = Request,
        Pick = Pick,
        CurrentTurn = CurrentTurn,
        DeckCount = Deck.Count,
        Pile = Pile,
        Hands = Hands.Select(h => h.ToData()).ToList(),
        Result = Result?.ToData(),
    };
    
    public static implicit operator GameData(Game game) => game.ToData();
}