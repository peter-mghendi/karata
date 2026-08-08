using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Karata.Platform.Infrastructure.Security;

public sealed class UserProvisioningClaimsTransformation(CurrentUserService current) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        await current.GetUserAsync(principal);
        return principal;
    }
}