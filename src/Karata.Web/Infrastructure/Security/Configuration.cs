using Karata.Shared.Security;

namespace Karata.Web.Infrastructure.Security;

// Karata.Web is a public client - there is no client secret since it uses the Authorization Code grant.
public static class Configuration
{
    public static Dictionary<string, ClientConfiguration> Client = new()
    {
        ["Development"] = new(Authority: "http://localhost:8080/realms/karata", Client: "karata-web"),
        ["Staging"] = new(Authority: "https://id.karata.app/realms/karata", Client: "karata-web"),
        ["Production"] = new(Authority: "https://id.karata.app/realms/karata", Client: "karata-web")
    };
}