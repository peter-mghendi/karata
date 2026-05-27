namespace Karata.Kit.Domain.Models;

public sealed record SystemMessage 
{
    public required string Text { get; init; }
    public MessageType Type { get; init; }

    public override string ToString() => $"[{Type.ToString().ToUpperInvariant()}] {Text}";
}