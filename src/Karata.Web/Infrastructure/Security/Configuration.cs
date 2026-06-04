using Karata.Kit.Application.Security;
using Karata.Web.Support;

namespace Karata.Web.Infrastructure.Security;

// Karata.Web is a public client - there is no client secret since it uses the Authorization Code grant.
public static class Configuration
{
    public static readonly Dictionary<string, ClientConfiguration> Client = new()
    {
        ["Development"] = new(Authority: "http://localhost:8080/realms/karata", Client: "karata-web"),
        ["Production"] = new(Authority: "https://id.karata.app/realms/karata", Client: "karata-web")
    };
    
    public static readonly Dictionary<string, ServerConfiguration> Server = new()
    {
        ["Development"] = new(Host: "https://localhost:7240"),
        ["Production"] = new(Host: "https://server.karata.app")
    };
}