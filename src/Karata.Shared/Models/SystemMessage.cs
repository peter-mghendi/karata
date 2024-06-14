namespace Karata.Shared.Models;

public record class SystemMessage 
{
    public required string Text { get; init; }
    public MessageType Type { get; init; }
}