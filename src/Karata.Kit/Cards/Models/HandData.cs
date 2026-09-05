using System.Text.Json.Serialization;
using Karata.Kit.Platform.Models;
using Karata.Pips;

namespace Karata.Kit.Cards.Models;

public record HandData
{
    public required long Id { get; init; }
    public required HandStatus Status { get; init; }
    public required UserData Player { get; init; }
    public required List<Card> Cards { get; init; } = [];
    public required bool IsLastCard { get; init; }

    [JsonIgnore] public string Username => Player.Username;
}