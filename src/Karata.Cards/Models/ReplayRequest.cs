namespace Karata.Cards.Models;

public sealed record ReplayRequest(Guid RoomId, string UserId, TimeSpan Interval, int StartTurn);
