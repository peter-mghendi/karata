using Karata.Kit.Platform.Models;
using RestSharp;

namespace Karata.Kit.Platform.Services;

public class ProfileService(RestClient client)
{
    public async Task<List<ProfileData>> ListAsync(CancellationToken cancellation = default)
    {
        // TODO [HTTP QUERY]: Change this to QueryAsync once RestClient has support
        var response = await client.GetAsync<List<ProfileData>>("profiles", cancellation);
        return response ?? throw new Exception();
    }

    public async Task<ProfileData?> GetAsync(string identifier) => await client.GetAsync<ProfileData>($"profiles/{identifier}");
}