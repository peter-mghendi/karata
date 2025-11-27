using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Karata.Server.Infrastructure.Security;

public static class CrossOrigin
{
    public static CorsPolicyBuilder AllowAll(CorsPolicyBuilder policy) => policy.AllowAnyOrigin().AllowAnyHeader();
}