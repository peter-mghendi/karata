using System.Net.Http.Headers;
using RestSharp;
using RestSharp.Interceptors;

namespace Karata.Kit.Security;

public sealed class BearerInterceptor(Func<Task<string?>> token) : Interceptor
{
    private const string Bearer = nameof(Bearer);

    public override async ValueTask BeforeHttpRequest(HttpRequestMessage message, CancellationToken cancellation)
    {
        message.Headers.Authorization = new AuthenticationHeaderValue(Bearer, await token());
    }
}

public static class RestClientOptionsExtensions
{
    extension(RestClientOptions options)
    {
        public void WithBearerInterceptor(Func<Task<string?>> token)
        {
            options.Interceptors = [new BearerInterceptor(token)];
        }
    }
} 