using RestSharp;

namespace Karata.Kit.Support.Http;

public abstract class BaseClient<TClientOptions>(TClientOptions options) where TClientOptions : IClientOptions
{
    protected RestClient Client { get; } = new(
        new Uri(options.Host, "/api"),
        client => client.WithBearerInterceptor(options.TokenProvider)
    );
}