using System.Net.Http.Json;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Interface;

/// <summary>
/// Interface for interacting with a Karata bot.
/// </summary>
/// <param name="http">
/// A <see cref="HttpClient"/> with the <see cref="HttpClient.BaseAddress"/> set to the base address of the bot.
/// </param>
public class BotInterface(HttpClient http)
{
    public async Task IntrospectAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task InviteAsync(BotInvitation invitation, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("/api/hands", invitation, ct);
        response.EnsureSuccessStatusCode();
    }
}