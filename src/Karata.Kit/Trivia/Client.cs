using Karata.Kit.Security;
using Karata.Kit.Trivia.Services;
using RestSharp;

namespace Karata.Kit.Trivia;

public class Client(Client.Options options)
{
    public sealed class Options
    {
        public required Uri Host { get; set; }

        public required Func<Task<string?>> TokenProvider { get; set; }
    }
    
    private readonly RestClient _client = new(new Uri(options.Host, "/api"), rest => rest.WithBearerInterceptor(options.TokenProvider));
    
    public GameService Games => new(_client);

    public NotificationService Notifications => new(_client);

    public ResponseService Responses => new(_client);

    public RoundService Rounds => new(_client);
    
    public TopicService Topics => new(_client);
    
    public UserService Users => new(_client);
}