using Karata.Kit.Cards.Models;
using Karata.Kit.Platform.Models;
using RestSharp;

namespace Karata.Kit.Platform.Services;

public class ProfileService(RestClient client)
{
    public async Task<List<ActivityData>> ListAsync(CancellationToken cancellation = default)
    {
        var response = await client.GetAsync<List<ActivityData>>("profiles", cancellation);
        return response ?? throw new Exception();
    }

    public async Task<ProfileData?> GetAsync(string username)
    {
        var response = await client.GetAsync<ProfileData>($"profiles/{username}");
        return response ?? throw new Exception();
    }
}