using Karata.Cards;

namespace Karata.Kit.Domain.Models;

public record HandData
{
    public required long Id { get; init; }
    public required HandStatus Status { get; init; }
    public required UserData Player { get; init; }
    public required List<Card> Cards { get; init; } = [];
}