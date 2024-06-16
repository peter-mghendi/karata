using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Hand
{
    public int Id { get; init; }
    public List<Card> Cards { get; init; } = [];
    public bool IsLastCard { get; set; }
    public int GameId { get; init; }
    public required User Player { get; init; }
    public List<Turn> Turns { get; init; } = [];

    public HandData ToData() => new()
    {
        Cards = Cards,
        User = Player.ToData(),
    };
}