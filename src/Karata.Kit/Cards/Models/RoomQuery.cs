namespace Karata.Kit.Cards.Models;

public sealed record RoomQuery(int Limit = 20, int Page = 1, RoomFilter? Filter = null);