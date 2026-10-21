namespace Karata.Kit.Cards.Models;

public sealed record RoomFilter(
    GameStatus[]? GameStatuses = null,
    RoomVisibility[]? Visibilities = null,
    int? MinHandCount = null,
    int? MaxHandCount = null,
    string[]? Creators = null,
    string[]? Administrators = null
);