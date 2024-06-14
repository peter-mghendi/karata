namespace Karata.Shared.Models;

public record RoomData
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public required UserData Creator { get; init; }
    public GameData Game { get; init; } = new ();
    public List<ChatData> Chats { get; init; } = [];
}