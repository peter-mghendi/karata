using Karata.Kit.Configuration;

namespace Karata.Cards;

public static class Configuration
{
    public static readonly Dictionary<string, HostConfiguration> Web = new()
    {
        ["Development"] = new(Host: "https://localhost:7240"),
        ["Production"] = new(Host: "https://web.karata.app")
    };
}