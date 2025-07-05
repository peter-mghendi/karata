using Karata.Shared.Models;

namespace Karata.Server.Models;

public class Room
{
    public Guid Id { get; init; }
    public required User Creator { get; init; }
    public Game Game { get; init; } = new();
    public required DateTimeOffset CreatedAt { get; set; }
    public byte[]? Hash { get; init; }
    public byte[]? Salt { get; init; }
    public List<Chat> Chats { get; init; } = [];

    public RoomData ToData() => new()
    {
        Id = Id,
        CreatedAt = CreatedAt,
        Creator = Creator.ToData(),
        Game = Game.ToData(),
        Chats = Chats.Select(c => c.ToData()).ToList()
    };
}