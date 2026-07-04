using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Surface.Security;

public static class TokenProvider
{
    public static async Task<string?> ProvideAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var result = await scope.ServiceProvider
            .GetRequiredService<IAccessTokenProvider>()
            .RequestAccessToken();
        return result.TryGetToken(out var token) ? token.Value : null;
    }
}