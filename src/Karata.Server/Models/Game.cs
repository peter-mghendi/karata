using System.ComponentModel.DataAnnotations.Schema;
using Karata.Shared.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.CardRequestLevel;

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

    public int NextTurn
    {
        get
        {
            var turn = CurrentHand.Turns.Last();
            var players = Hands.Count;
            var skip = (int)turn.Delta.Skip % players;

            return IsReversed
                ? (CurrentTurn - skip + players) % players
                : (CurrentTurn + skip) % players;
        }
    }

    public Deck Deck { get; init; } = Deck.Standard;
    public Pile Pile { get; init; } = new();

    public GameResult? Result { get; set; }

    public Guid RoomId { get; set; }

    public List<Hand> Hands { get; set; } = [];

    [NotMapped] public Hand CurrentHand => Hands[CurrentTurn];

    public HashSet<Hand> HandsExceptPlayerId(string playerId) => Hands.Where(h => h.Player.Id != playerId).ToHashSet();

    public GameData ToData() => new()
    {
        IsForward = IsReversed,
        Status = Status,
        CurrentRequest = Request,
        CurrentTurn = CurrentTurn,
        DeckCount = Deck.Count,
        Pile = Pile,
        Hands = Hands.Select(h => h.ToData()).ToList(),
        Result = Result?.ToData(),
    };
}