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
    public async Task<BotData> IntrospectAsync(CancellationToken cancellation = default)
    {
        var response = await http.GetFromJsonAsync<BotData>("", cancellation);
        return response ?? throw new Exception($"Unable to fetch details for bot: {http.BaseAddress}.");
    }

    public async Task InviteAsync(BotInvitation invitation, CancellationToken cancellation   = default)
    {
        var response = await http.PostAsJsonAsync("/hands", invitation, cancellation);
        response.EnsureSuccessStatusCode();
    }
}