using System.Net.Http.Json;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Bot.Interface;

/// <summary>
/// Interface for interacting with a Karata bot.
/// </summary>
/// <param name="http">
/// A <see cref="HttpClient"/> with the <see cref="HttpClient.BaseAddress"/> set to the base address of the bot.
/// </param>
public class BotInterface(HttpClient http, string identifier)
{
    public async Task<BotData> IntrospectAsync(CancellationToken ct = default)
    {
        var response = await http.GetFromJsonAsync<BotData>($"/api/bots/{identifier}", ct);
        return response ?? throw new Exception($"Unable to fetch details for bot: {identifier}.");
    }

    public async Task InviteAsync(BotInvitation invitation, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync($"/api/bots/{identifier}/hands", invitation, ct);
        response.EnsureSuccessStatusCode();
    }
}