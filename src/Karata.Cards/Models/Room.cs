using Karata.Kit.Cards.Models;
using static Karata.Kit.Cards.Models.HandStatus;

namespace Karata.Cards.Models;

public class Room
{
    public Guid Id { get; init; }
    public required RoomVisibility Visibility { get; set; }
    public required User Administrator { get; set; }
    public required User Creator { get; set; }
    public Game Game { get; init; } = new();
    public required DateTimeOffset CreatedAt { get; set; }
    public List<Chat> Chats { get; init; } = [];

    public User? NextEligibleAdministrator => Game.Hands
        .Where(hand => hand.Player.Id != Administrator.Id)
        .Where(hand => hand.Status is Active)
        .OrderBy(hand => (int)hand.Status)
        .ThenBy(hand => hand.Id)
        .FirstOrDefault()?
        .Player;

    public RoomData ToData() => new()
    {
        Id = Id,
        Visibility = Visibility,
        CreatedAt = CreatedAt,
        Administrator = Administrator,
        Creator = Creator,
        Game = Game,
        Chats = [..Chats.Select(c => c.ToData())]
    };
}