using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Karata.Trivia.Infrastructure;

public sealed class UserProvisioningClaimsTransformation(CurrentUserService current) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        await current.GetUserAsync(principal);
        return principal;
    }
}