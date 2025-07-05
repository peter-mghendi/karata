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
    public int HandId { get; init; }
}