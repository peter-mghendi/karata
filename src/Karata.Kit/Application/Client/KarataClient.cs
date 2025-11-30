using System.Net.Http.Headers;
using Karata.Kit.Application.Client.Services;
using RestSharp;
using RestSharp.Interceptors;

namespace Karata.Kit.Application.Client;

public class KarataClient(Uri host, Func<Task<string?>> token)
{
    private readonly RestClient _client = new(new Uri(host, "/api"), options => options.Interceptors = [new BearerInterceptor(token)]);

    public ActivityService Activity => new(_client);
    public RoomService Rooms => new(_client);
    public TurnService Turns => new(_client);
}

file sealed class BearerInterceptor(Func<Task<string?>> token) : Interceptor
{
    private const string Bearer = nameof(Bearer);

    public override async ValueTask BeforeHttpRequest(HttpRequestMessage message, CancellationToken cancellation)
    {
        message.Headers.Authorization = new AuthenticationHeaderValue(Bearer, await token());
    }
}