using static Karata.Cards.Card.CardFace;
using static Karata.Shared.Models.CardRequestLevel;

namespace Karata.Shared.Models;

public record GameData
{
    public bool IsReversed { get; set; } = true;
    public GameStatus Status { get; set; }
    public Card? Request { get; set; }
    public uint Pick { get; set; }
    public int CurrentTurn { get; set; }
    public Deck Deck { get; set; } = [];
    public Pile Pile { get; set; } = [];
    public List<HandData> Hands { get; set; } = [];
    public GameResultData? Result { get; set; }

    public HandData CurrentHand => Hands[CurrentTurn];

    public CardRequestLevel RequestLevel => Request switch
    {
        null => NoRequest,
        { Face: None } => SuitRequest,
        { Face: not None } => CardRequest
    };
}