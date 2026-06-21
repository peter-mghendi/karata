using Karata.Kit.Application.Security;
using Karata.Surface.Models;

namespace Karata.Web;

// Karata.Web is a public client - there is no client secret since it uses the Authorization Code grant.
public static class Configuration
{
    public static readonly Dictionary<string, ClientConfiguration> Client = new()
    {
        ["Development"] = new(Authority: "http://localhost:8080/realms/karata", Id: "karata-web"),
        ["Production"] = new(Authority: "https://id.karata.app/realms/karata", Id: "karata-web")
    };
    
    public static readonly Dictionary<string, ServerConfiguration> Server = new()
    {
        ["Development"] = new(Host: "https://localhost:7240"),
        ["Production"] = new(Host: "https://server.karata.app")
    };
    
    public static readonly Dictionary<string, BotInterfaceConfiguration> BotInterface = new()
    {
        ["Development"] = new(Host: "https://localhost:7242"),
        ["Production"] = new(Host: "https://bot.karata.app")
    };
}