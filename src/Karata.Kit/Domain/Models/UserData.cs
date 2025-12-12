namespace Karata.Kit.Domain.Models;

public record UserData {
    public required string Id { get; set; }
    public required string Username { get; set; }
}