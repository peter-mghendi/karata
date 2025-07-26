using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Turn
{
    public int Id { get; init; }
    public List<Card> Cards { get; init; } = [];
    public List<Card> Picked { get; set; } = [];
    public Card? Request { get; set; }  
    public bool IsLastCard { get; set; }
    public GameDelta? Delta { get; init; }
    public required TurnType Type { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required Hand Hand { get; init; }
    public int HandId { get; init; }
    
    public TurnData ToData() => new()
    {
        Id = Id,
        Cards = Cards,
        Picked = Picked,
        Request = Request,
        IsLastCard = IsLastCard,
        Delta = Delta,
        Type = Type,
        CreatedAt = CreatedAt,
        Hand = Hand.ToData()
    };
}