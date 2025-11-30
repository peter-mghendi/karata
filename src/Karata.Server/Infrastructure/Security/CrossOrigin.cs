using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Karata.Server.Infrastructure.Security;

public static class CrossOrigin
{
    public static void AllowAll(CorsPolicyBuilder policy) => policy.AllowAnyOrigin().AllowAnyHeader();
}