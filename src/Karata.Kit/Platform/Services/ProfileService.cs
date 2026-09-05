using Karata.Kit.Platform.Models;
using RestSharp;

namespace Karata.Kit.Platform.Services;

public class ProfileService(RestClient client)
{
    public async Task<List<UserData>> ListAsync(CancellationToken cancellation = default)
    {
        // TODO [HTTP QUERY]: Change this to QueryAsync once RestClient has support
        var response = await client.GetAsync<List<UserData>>("profiles", cancellation);
        return response ?? throw new Exception();
    }

    public async Task<UserData?> GetAsync(string identifier) => await client.GetAsync<UserData>($"profiles/{identifier}");
}