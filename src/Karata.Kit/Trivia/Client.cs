using Karata.Kit.Security;
using Karata.Kit.Support.Http;
using Karata.Kit.Trivia.Services;
using RestSharp;

namespace Karata.Kit.Trivia;

public sealed class Client(Client.Options options) : BaseClient<Client.Options>(options)
{
    public sealed class Options : IClientOptions
    {
        public required Uri Host { get; set; }

        public required Func<Task<string?>> TokenProvider { get; set; }
    }
    
    public GameService Games => new(Client);

    public ResponseService Responses => new(Client);

    public RoundService Rounds => new(Client);
    
    public TopicService Topics => new(Client);
    
    public UserService Users => new(Client);
}