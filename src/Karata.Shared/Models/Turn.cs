namespace Karata.Shared.Models;

public class TurnData
{
    public int Id { get; init; }
    public required List<Card> Cards { get; init; }
    public required List<Card> Picked { get; set; }
    public Card? Request { get; set; }
    public bool IsLastCard { get; set; }
    public GameDelta? Delta { get; init; }
    public required TurnType Type { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required HandData Hand { get; init; }
}