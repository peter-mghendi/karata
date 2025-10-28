using System.Collections.Concurrent;
using System.Security.Claims;
using Karata.Server.Data;
using Microsoft.Extensions.Options;

namespace Karata.Server.Infrastructure.Security;

public sealed class CurrentUserService(
    IHttpContextAccessor http,
    KarataContext context,
    IOptions<UserProvisioningOptions> options
)
{
    private const string UserNotFound = "Authenticated user not found and auto-provisioning is disabled or failed.";
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    private readonly UserProvisioningOptions _options = options.Value;

    public async Task<User> RequireAsync(ClaimsPrincipal? principal = null, CancellationToken ct = default)
        => await GetUserAsync(principal, ct) ?? throw new UnauthorizedAccessException(UserNotFound);

    public async Task<User?> GetUserAsync(ClaimsPrincipal? principal = null, CancellationToken ct = default)
    {
        if ((principal ??= http.HttpContext?.User) is null or { Identity.IsAuthenticated: false }) return null;
        if (principal!.FindFirstValue(ClaimTypes.NameIdentifier) is not [_, ..] userId) return null;
        
        var gate = Locks.GetOrAdd(userId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            if (await context.Users.FindAsync([userId], cancellationToken: ct) is { } user) return user;
            if (!_options.AutoProvisionEnabled) return null;

            user = context.Users.Add(_options.NewUserFactory(principal!)).Entity;

            await context.SaveChangesAsync(ct);
            return user;
        }
        finally
        {
            gate.Release();
            if (gate.CurrentCount == 1) Locks.TryRemove(new KeyValuePair<string, SemaphoreSlim>(userId, gate));
        }
    }
}