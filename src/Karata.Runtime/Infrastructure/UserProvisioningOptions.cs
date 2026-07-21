using System.Security.Claims;
using Karata.Runtime.Models;

namespace Karata.Runtime.Infrastructure;

public sealed record DatabaseOptions
{
    public required Uri DataSource { get; set; }
}

public sealed class UserProvisioningOptions<TUser> where TUser : KarataUser
{
    public bool Enabled { get; set; }

    public required Func<ClaimsPrincipal, TUser> Factory { get; set; }
}