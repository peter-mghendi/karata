using System.Security.Claims;
using Karata.Runtime.Data;
using Karata.Runtime.Models;
using Microsoft.AspNetCore.Authentication;

namespace Karata.Runtime.Infrastructure;

public sealed class UserProvisioningClaimsTransformation<TContext, TUser>(CurrentUserService<TContext, TUser> current)
    : IClaimsTransformation
    where TContext : KarataContext<TUser>
    where TUser : KarataUser

{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        await current.GetUserAsync(principal);
        return principal;
    }
}