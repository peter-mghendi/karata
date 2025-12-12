namespace Karata.Kit.Domain.Models;

public record RoomRequest
{
    public required string Password { get; init; }
}