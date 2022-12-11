namespace Karata.Shared.Models;

public record class SystemMessage 
{
    public string Text { get; init; } = string.Empty;
    public MessageType Type { get; init; }
}