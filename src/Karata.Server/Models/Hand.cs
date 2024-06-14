using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Hand
{
    public int Id { get; set; }
    public List<Card> Cards { get; set; } = new();
    public bool IsLastCard { get; set; }
    public int GameId { get; set; }
    public required User User { get; set; }

    public HandData ToData() => new()
    {
        Cards = Cards,
        User = User.ToData(),
    };
}