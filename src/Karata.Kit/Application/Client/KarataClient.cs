using System.Net.Http.Headers;
using Karata.Kit.Application.Client.Services;
using RestSharp;
using RestSharp.Interceptors;

namespace Karata.Kit.Application.Client;

file sealed class BearerInterceptor(Func<Task<string?>> token) : Interceptor
{
    private const string Bearer = nameof(Bearer);

    public override async ValueTask BeforeHttpRequest(HttpRequestMessage message, CancellationToken cancellation)
    {
        message.Headers.Authorization = new AuthenticationHeaderValue(Bearer, await token());
    }
}

file static class RestClientOptionsExtensions
{
    extension(RestClientOptions options)
    {
        public void WithBearerInterceptor(Func<Task<string?>> token)
        {
            options.Interceptors = [new BearerInterceptor(token)];
        }
    }
} 

public class KarataClient(KarataClientOptions options)
{
    private readonly RestClient _client = new(new Uri(options.Host, "/api"), rest => rest.WithBearerInterceptor(options.TokenProvider));

    public ActivityService Activity => new(_client);

    public RoomService Rooms => new(_client);

    public TurnService Turns => new(_client);
}
