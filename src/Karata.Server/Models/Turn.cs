namespace Karata.Server.Models;

public class Turn
{
    public int Id { get; set; }
    public bool IsLastCard { get; set; }
    public List<Card> Cards { get; set; } = new();
    public Card? Request { get; set; }  
    public GameDelta? Delta { get; set; }
    public required string UserId { get; set; }
    public int GameId { get; set; }
}