using Karata.Shared.Models;
using RestSharp;

namespace Karata.Shared.Client.Services;

public class TurnService(RestClient client)
{
    public async Task<List<TurnData>> ListAsync(Guid room, CancellationToken cancellation = default)
    {
        var response = await client.GetAsync<List<TurnData>>($"rooms/{room}/turns", cancellation);
        return response ?? throw new Exception();
    }
}