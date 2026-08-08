namespace Karata.Kit.Cards.Models;

public sealed record UserData {
    public required string Id { get; set; }
    public required string Username { get; set; }
}