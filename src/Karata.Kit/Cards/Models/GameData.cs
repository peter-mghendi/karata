using Karata.Pips;
using static Karata.Kit.Cards.Models.CardRequestLevel;
using static Karata.Pips.Card.CardFace;

namespace Karata.Kit.Cards.Models;

public record GameData
{
    public bool IsReversed { get; set; } = true;
    public GameStatus Status { get; set; }
    public Card? Request { get; set; }
    public uint Give { get; set; }
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