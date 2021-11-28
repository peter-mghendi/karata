#nullable enable

namespace Karata.Web.Models;

public class Turn
{
    public int Id { get; set; }
    public List<Card>? Cards { get; set; }
    public bool IsLastCard { get; set; } = false;
    public Card? Request { get; set; } = null;
    public string? UserId { get; set; }
    public int GameId { get; set; }
}