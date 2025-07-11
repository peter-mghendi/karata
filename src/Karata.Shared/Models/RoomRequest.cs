namespace Karata.Shared.Models;

public record RoomRequest
{
    public required string Password { get; init; }
}