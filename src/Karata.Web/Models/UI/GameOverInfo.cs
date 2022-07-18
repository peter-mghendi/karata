#nullable enable

namespace Karata.Web.Models.UI;

public record GameOverInfo(string RoomId, string Reason, UIUser? Winner);