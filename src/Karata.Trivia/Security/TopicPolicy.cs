using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Karata.Trivia.Security;

public static class TopicPolicy
{
    public static void ManageTopics(AuthorizationPolicyBuilder policy) => policy.RequireRealmRoles(RealmRoles.Admin);
}