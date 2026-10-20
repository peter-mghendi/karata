using Karata.Kit.Cards.Models;
using Karata.Kit.Common.Models;
using Karata.Kit.Support.Http;
using RestSharp;

namespace Karata.Kit.Cards.Services;

public class RoomService(RestClient client)
{
    public async Task<Page<RoomData>> QueryAsync(RoomQuery query, CancellationToken cancellation = default)
    {
        // TODO: [HTTP QUERY]: RestSharp does not yet have support for RFC 10008 HTTP QUERY
        var request = new RestRequest("rooms", Method.Post);
        request.AddJsonBody(query);
        request.Interceptors = [new QueryInterceptor()];

        return await client.ExecuteAsync<Page<RoomData>>(request, cancellation) switch
        {
            { IsSuccessful: true } and { Data: { } data } => data,
            { } response => throw new Exception($"Query failed: {response.ErrorMessage}")
        };
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