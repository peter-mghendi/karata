using Karata.Cards;

namespace Karata.Kit.Domain.Models;

public class TurnData
{
    public int Id { get; init; }
    public required List<Card> CardsPlayed { get; init; }
    public required List<Card> CardsPicked { get; set; }
    public Card? Request { get; set; }
    public bool IsLastCard { get; set; }
    public TurnDelta? Delta { get; init; }
    public required TurnType Type { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required HandData Hand { get; init; }
}