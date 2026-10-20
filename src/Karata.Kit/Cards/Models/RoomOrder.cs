using Karata.Kit.Common.Models;
using Karata.Kit.Support;

namespace Karata.Kit.Cards.Models;

public sealed record RoomOrder(
    RoomOrderField Field = RoomOrderField.CreatedAt,
    SortDirection Direction = SortDirection.Descending
);