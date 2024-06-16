namespace Karata.Server.Models;

public class Turn
{
    public int Id { get; init; }
    public List<Card> Cards { get; init; } = [];
    public Card? Request { get; set; }  
    public bool IsLastCard { get; set; }
    public required GameDelta Delta { get; init; }
    public int HandId { get; init; }
}