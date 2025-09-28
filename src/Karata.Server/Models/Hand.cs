using System.Text.Json.Serialization;
using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Hand
{
    public int Id { get; init; }
    public required HandStatus Status { get; set; }
    public List<Card> Cards { get; init; } = [];
    public bool IsLastCard { get; set; }
    public int GameId { get; init; }
    public required User Player { get; init; }
    
    [JsonIgnore]
    public List<Turn> Turns { get; init; } = [];

    public HandData ToData() => new()
    {
        Id = Id,
        Status = Status,
        Cards = [..Enumerable.Repeat(new Card(), Cards.Count)],
        Player = Player.ToData(),
    };
}