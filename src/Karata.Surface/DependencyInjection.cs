using Blazored.LocalStorage;
using Karata.Kit.Security;
using Karata.Surface.Security;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudExtensions.Services;
using TextCopy;

namespace Karata.Surface;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataOidc(ClientConfiguration client)
        {
            services.AddSingleton(client);
            services.AddOidcAuthentication(options =>
            {
                // options.ProviderOptions.DefaultScopes.Add("Audience");
                options.ProviderOptions.Authority = client.Authority;
                options.ProviderOptions.ClientId = client.Id;
                options.ProviderOptions.MetadataUrl = $"{client.Authority}/.well-known/openid-configuration";
                options.ProviderOptions.ResponseType = "id_token token";
                options.UserOptions.NameClaim = "preferred_username";
                options.UserOptions.RoleClaim = "roles";
                options.UserOptions.ScopeClaim = "scope";
            });
            
            return services;
        }
        
        public IServiceCollection AddKarataSurface()
        {
            services.AddBlazoredLocalStorage();
            services.InjectClipboard();
            services.AddMudServices();
            services.AddMudExtensions();
            services.AddScoped<AuthenticationHelper>();
            
            return services;
        }
    }
}