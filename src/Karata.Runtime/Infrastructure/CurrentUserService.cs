using System.Security.Claims;
using Karata.Runtime.Data;
using Karata.Runtime.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Karata.Runtime.Infrastructure;

public sealed class CurrentUserService<TContext, TUser>(
    IHttpContextAccessor http,
    TContext context,
    IOptions<UserProvisioningOptions<TUser>> options
)
    where TContext : KarataContext<TUser>
    where TUser : KarataUser
{
    private const string UserNotFound = "Authenticated user not found and auto-provisioning is disabled or failed.";

    private readonly UserProvisioningOptions<TUser> _options = options.Value;

    public async Task<TUser> RequireAsync(ClaimsPrincipal? principal = null, CancellationToken ct = default)
        => await GetUserAsync(principal, ct) ?? throw new UnauthorizedAccessException(UserNotFound);

    public async Task<TUser?> GetUserAsync(ClaimsPrincipal? principal = null, CancellationToken ct = default)
    {
        if ((principal ??= http.HttpContext?.User) is null or { Identity.IsAuthenticated: false }) return null;
        if (principal!.FindFirstValue(ClaimTypes.NameIdentifier) is not [_, ..] userId) return null;

        if (await context.Users.FindAsync([userId], cancellationToken: ct) is { } user) return user;
        if (!_options.Enabled) return null;

        user = context.Users.Add(_options.Factory(principal!)).Entity;

        try
        {
            await context.SaveChangesAsync(ct);
            return user;
        }
        catch (DbUpdateException)
        {
            context.Entry(user).State = EntityState.Detached;
            if (await context.Users.FindAsync([userId], cancellationToken: ct) is { } existing) return existing;

            throw;
        }
    }
}