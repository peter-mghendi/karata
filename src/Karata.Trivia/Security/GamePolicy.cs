using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Karata.Trivia.Security;

public static class GamePolicy
{
    public static void QueryGames(AuthorizationPolicyBuilder policy) => policy.RequireRealmRoles("trivia:admin");
}