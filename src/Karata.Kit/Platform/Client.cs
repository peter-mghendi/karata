using Karata.Kit.Platform.Services;
using Karata.Kit.Security;
using RestSharp;

namespace Karata.Kit.Platform;

public class Client(Client.Options options)
{
    public sealed class Options
    {
        public required Uri Host { get; set; }

        public required Func<Task<string?>> TokenProvider { get; set; }
    }
    
    private readonly RestClient _client = new(new Uri(options.Host, "/api"), rest => rest.WithBearerInterceptor(options.TokenProvider));

    public ActivityService Activity => new(_client);
    
    public ProfileService Profiles => new(_client);
}
