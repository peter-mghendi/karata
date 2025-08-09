using Karata.Shared.Models;
using static Karata.Shared.Models.HandStatus;

namespace Karata.Server.Models;

public class Room
{
    public Guid Id { get; init; }
    public required User Administrator { get; set; }
    public required User Creator { get; set; }
    public Game Game { get; init; } = new();
    public required DateTimeOffset CreatedAt { get; set; }
    public byte[]? Hash { get; set; }
    public byte[]? Salt { get; set; }
    public List<Chat> Chats { get; init; } = [];

    public User? NextEligibleAdministrator => Game.Hands
        .Where(hand => hand.Player.Id != Administrator.Id)
        .Where(hand => hand.Status is Online or Offline)
        .OrderBy(hand => (int)hand.Status)
        .ThenBy(hand => hand.Id)
        .FirstOrDefault()?
        .Player;

    public RoomData ToData() => new()
    {
        Id = Id,
        CreatedAt = CreatedAt,
        Administrator = Administrator,
        Creator = Creator,
        Game = Game,
        Chats = Chats.Select(c => c.ToData()).ToList()
    };
}