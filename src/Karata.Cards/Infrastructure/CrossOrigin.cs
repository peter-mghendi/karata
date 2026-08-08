using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Karata.Cards.Infrastructure;

public static class CrossOrigin
{
    public static void AllowAll(CorsPolicyBuilder policy) => policy.AllowAnyOrigin().AllowAnyHeader();
}