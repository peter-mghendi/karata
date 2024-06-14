namespace Karata.Shared.Models;

public record GameOverInfo(string RoomId, string Reason, UserData? Winner);