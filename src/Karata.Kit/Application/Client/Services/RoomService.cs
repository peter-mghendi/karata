using Karata.Kit.Domain.Models;
using RestSharp;

namespace Karata.Kit.Application.Client.Services;

public class RoomService(RestClient client)
{
    public async Task<List<RoomData>> ListAsync(CancellationToken cancellation = default)
    {
        var response = await client.GetAsync<List<RoomData>>("rooms", cancellation);
        return response ?? throw new Exception();
    }
    
    public async Task<RoomData> CreateAsync(RoomRequest request, CancellationToken cancellation = default)
    {
        var response = await client.PostJsonAsync<RoomRequest, RoomData>("rooms", request, cancellation);
        return response ?? throw new Exception();
    }
    
    public async Task<RoomData> GetAsync(Guid room, CancellationToken cancellation = default)
    {
        var response = await client.GetAsync<RoomData>($"rooms/{room}", cancellation);
        return response ?? throw new Exception();
    }
}