using Karata.Kit.Security;
using Karata.Surface.Models;

namespace Karata.Desktop;

// Karata.Desktop is a public client - there is no client secret since it uses the Authorization Code grant.
public static class Configuration
{
    public static readonly Dictionary<string, ClientConfiguration> Client = new()
    {
        ["Development"] = new(Authority: "http://localhost:18080/realms/karata", Id: "karata-desktop"),
        ["Production"] = new(Authority: "https://id.karata.app/realms/karata", Id: "karata-desktop")
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