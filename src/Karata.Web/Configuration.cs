using Karata.Kit.Security;
using Karata.Surface.Models;

namespace Karata.Web;

// Karata.Web is a public client - there is no client secret since it uses the Authorization Code grant.
public static class Configuration
{
    public static readonly Dictionary<string, ClientConfiguration> Client = new()
    {
        ["Development"] = new(
            Id: "karata-web",
            Authority: "http://localhost:18080/realms/karata",
            Audiences: ["karata-cards", "karata-platform"]
        ),
        ["Production"] = new(
            Id: "karata-web",
            Authority: "https://id.karata.app/realms/karata",
            Audiences: ["karata-cards", "karata-platform"]
        )
    };
    
    public static readonly Dictionary<string, HostConfiguration> Bot = new()
    {
        ["Development"] = new(Host: "https://localhost:7242/api/bots/random"),
        ["Production"] = new(Host: "https://bot.karata.app/api/bots/random")
    };

    public static readonly Dictionary<string, HostConfiguration> Cards = new()
    {
        ["Development"] = new(Host: "https://localhost:7240"),
        ["Production"] = new(Host: "https://cards.karata.app")
    };

    public static readonly Dictionary<string, HostConfiguration> Platform = new()
    {
        ["Development"] = new(Host: "https://localhost:7243"),
        ["Production"] = new(Host: "https://platform.karata.app")
    };
}