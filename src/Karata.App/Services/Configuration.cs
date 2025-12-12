using Karata.Kit.Application.Security;

namespace Karata.App.Services;

// Karata.App is a public client - there is no client secret since it uses the Authorization Code grant.
public static class Configuration
{
#if PRODUCTION
    public static readonly ClientConfiguration Client = new
    (
        Authority: "https://id.karata.app/realms/karata",
        Client: "karata-app"
    );
#elif STAGING
    public static readonly ClientConfiguration Client = new(
        Authority: "https://id.karata.app/realms/karata",
        Client: "karata-app"
    );
#else
    public static readonly ClientConfiguration Client = new
    (
        Authority: "http://localhost:8080/realms/karata",
        Client: "karata-app"
    );
#endif
}