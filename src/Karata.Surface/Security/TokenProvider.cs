using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Surface.Security;

public static class TokenProvider
{
    public static async Task<string?> ProvideAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IAccessTokenProvider>();
        var result = await provider.RequestAccessToken();
        
        return result.TryGetToken(out var token) switch
        {
            true => token.Value,
            false => null
        };
    }
}