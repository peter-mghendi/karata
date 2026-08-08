namespace Karata.Kit.Bot.Models;

public sealed record BotInvitation(Guid Room, string? Password = null);