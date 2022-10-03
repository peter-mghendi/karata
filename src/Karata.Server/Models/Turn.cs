namespace Karata.Server.Models;

public class Turn
{
    public int Id { get; set; }
    public List<Card> Cards { get; set; } = new();
    public bool IsLastCard { get; set; } = false;
    public Card? Request { get; set; } = null;
    public string UserId { get; set; } = null!;
    public int GameId { get; set; }
}