using Karata.Kit.Bot.Models;
using RestSharp;
using static System.Net.HttpStatusCode;

namespace Karata.Kit.Bot.Interface;

/// <summary>Interface for interacting with a Karata bot.</summary>
/// <param name="host">The base address of the bot.</param>
public sealed class BotInterface(Uri host)
{
    private readonly RestClient _client = new(host);
    
    public async Task<BotData> IntrospectAsync(CancellationToken cancellation = default)
    {
        var response = await _client.GetAsync<BotData>("", cancellation);
        return response ?? throw new Exception($"Unable to fetch details for bot '{host}'.");
    }

    public async Task InviteAsync(BotInvitation invitation, CancellationToken cancellation   = default)
    {
        var response = await _client.PostJsonAsync($"/games", invitation, cancellation);
        if (response is not Accepted) throw new Exception($"{response} - Unable to invite bot '{host}: {invitation}'");
    }
}