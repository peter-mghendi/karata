using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Karata.Platform.Infrastructure.Security;

public static class CrossOrigin
{
    public static void AllowAll(CorsPolicyBuilder policy) => policy.AllowAnyOrigin().AllowAnyHeader();
}