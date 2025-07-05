namespace Karata.Shared.Models;

public record RoomData
{
    public Guid Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required UserData Creator { get; init; }
    public GameData Game { get; init; } = new ();
    public List<ChatData> Chats { get; init; } = [];
}