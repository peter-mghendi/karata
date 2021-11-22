#nullable enable

namespace Karata.Web.Models;

public record class SystemMessage(string Text, MessageType Type) { }