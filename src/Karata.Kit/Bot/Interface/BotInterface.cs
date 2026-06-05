using Karata.Kit.Domain.Models;
using RestSharp;
using static System.Net.HttpStatusCode;

namespace Karata.Kit.Bot.Interface;

/// <summary>Interface for interacting with a Karata bot.</summary>
/// <param name="host">The base address of the bot.</param>
public sealed class BotInterface(Uri host)
{
    private readonly RestClient _client = new(host);
    
    public async Task<BotData> IntrospectAsync(string bot, CancellationToken cancellation = default)
    {
        var response = await _client.GetAsync<BotData>($"/api/bots/{bot}", cancellation);
        return response ?? throw new Exception($"Unable to fetch details for bot '{bot}'.");
    }

    public async Task InviteAsync(string bot, BotInvitation invitation, CancellationToken cancellation   = default)
    {
        var response = await _client.PostJsonAsync($"/api/bots/{bot}/games", invitation, cancellation);
        if (response is not Accepted) throw new Exception($"Unable to invite bot '{bot}': {response}");
    }
}