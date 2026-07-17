using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Karata.Trivia.Infrastructure;

public static class CrossOrigin
{
    public static void AllowAll(CorsPolicyBuilder policy) => policy.AllowAnyOrigin().AllowAnyHeader();
}