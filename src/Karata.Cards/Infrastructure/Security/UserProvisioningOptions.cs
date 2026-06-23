using System.Security.Claims;
using Karata.Cards.Models;

namespace Karata.Cards.Infrastructure.Security;

public sealed class UserProvisioningOptions
{
    public bool AutoProvisionEnabled { get; set; }

    public Func<ClaimsPrincipal, User> NewUserFactory { get; set; } = principal => new User
    {
        Id = principal.FindFirstValue(ClaimTypes.NameIdentifier)!,
        // Email = principal.FindFirstValue(ClaimTypes.Email)!,
        Username = principal.FindFirstValue("preferred_username")!
    };
}