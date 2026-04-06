using Karata.Kit.Domain.Models;

namespace Karata.Server.Models;

public class Turn
{
    public int Id { get; init; }
    public List<Card> CardsPlayed { get; init; } = [];
    public List<Card> CardsPicked { get; set; } = [];
    public Card? Request { get; set; }  
    public bool IsCardless { get; set; }
    public bool IsLastCard { get; set; }
    public bool ReclaimedPile { get; set; }
    public bool DeckExhausted { get; set; }
    public TurnDelta? Delta { get; set; }
    public GameResult? GameResult { get; set; }
    public required TurnType Type { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
    public GameData? GameSnapshot { get; set; }
    public TurnMetadata Metadata { get; init; } = new();
    public required Hand Hand { get; init; }
    public int HandId { get; init; }
    
    public TurnData ToData() => new()
    {
        Id = Id,
        CardsPlayed = CardsPlayed,
        CardsPicked = CardsPicked,
        Request = Request,
        IsLastCard = IsLastCard,
        Delta = Delta,
        Type = Type,
        CreatedAt = CreatedAt,
        Hand = Hand.ToData()
    };
}