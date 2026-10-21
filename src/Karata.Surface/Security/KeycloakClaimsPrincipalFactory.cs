using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace Karata.Surface.Security;

public class KeycloakClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
    : AccountClaimsPrincipalFactory<RemoteUserAccount>(accessor)
{
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(
        RemoteUserAccount account,
        RemoteAuthenticationUserOptions options
    )
    {
        var user = await base.CreateUserAsync(account, options);
        if (user.Identity is not { IsAuthenticated: true })
            return user;

        var identity = (ClaimsIdentity)user.Identity;
        if (!account.AdditionalProperties.TryGetValue("realm_access", out var json) || json is not JsonElement element)
            return user;

        identity.AddRolesFromElement(element);
        return user;
    }
}

file static class RoleMapper
{
    extension(ClaimsIdentity identity)
    {
        public void AddRolesFromElement(JsonElement parentElement)
        {
            if (!parentElement.TryGetProperty("roles", out var roles) ||
                roles.ValueKind is not JsonValueKind.Array) return;

            var claims = from role in roles.EnumerateArray()
                let value = role.GetString()
                where value is [_, ..] && !identity.HasClaim(identity.RoleClaimType, value)
                select new Claim(identity.RoleClaimType, value);

            identity.AddClaims(claims);
        }
    }
}