namespace Karata.Shared.Models;

public record UserData {
    public required string Id { get; set; }
    public required string Email { get; set; }
}