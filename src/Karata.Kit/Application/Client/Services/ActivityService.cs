using Karata.Kit.Domain.Models;
using RestSharp;

namespace Karata.Kit.Application.Client.Services;

public class ActivityService(RestClient client)
{
    public async Task<List<ActivityData>> ListAsync(CancellationToken cancellation = default)
    {
        var response = await client.GetAsync<List<ActivityData>>("activity", cancellation);
        return response ?? throw new Exception();
    }
}