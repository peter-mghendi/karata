using Karata.Kit.Platform.Services;
using Karata.Kit.Support.Http;

namespace Karata.Kit.Platform;

public sealed class Client(Client.Options options) : BaseClient<Client.Options>(options)
{
    public sealed class Options : IClientOptions
    {
        public required Uri Host { get; set; }

        public required Func<Task<string?>> TokenProvider { get; set; }
    }

    public ActivityService Activity => new(Client);
    
    public ProfileService Profiles => new(Client);
}
