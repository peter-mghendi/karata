using Karata.Kit.Cards.Models;
using RestSharp;

namespace Karata.Kit.Cards.Services;

public class RoomHandsService(RestClient client)
{    
    public async Task CreateAsync(Guid room, HandRequest request, CancellationToken cancellation = default)
    {
        _ = await client.PostJsonAsync($"rooms/{room}/hands", request, cancellation);
    }
}