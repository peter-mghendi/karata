using Karata.Kit.Platform.Models;
using RestSharp;

namespace Karata.Kit.Platform.Services;

public class ActivityService(RestClient client)
{
    public async Task<List<ActivityData>> ListAsync(CancellationToken cancellation = default)
    {
        var response = await client.GetAsync<List<ActivityData>>("activity", cancellation);
        return response ?? throw new Exception();
    }
    
    public async Task<ActivityData> CreateAsync(ActivityRequest request, CancellationToken cancellation = default)
    {
        var response = await client.PostJsonAsync<ActivityRequest, ActivityData>("activity", request, cancellation);
        return response ?? throw new Exception();
    }}