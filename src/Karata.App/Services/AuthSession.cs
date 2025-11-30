using System.Security.Claims;

namespace Karata.App.Services;

internal sealed record AuthSession(
    string AccessToken,
    string IdentityToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    ClaimsPrincipal User
);