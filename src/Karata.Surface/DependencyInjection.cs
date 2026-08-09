using Blazored.LocalStorage;
using Howler.Blazor.Components;
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
                options.ProviderOptions.Authority = client.Authority;
                options.ProviderOptions.ClientId = client.Id;
                options.ProviderOptions.MetadataUrl = $"{client.Authority}/.well-known/openid-configuration";
                options.ProviderOptions.ResponseType = "code";

                foreach (var audience in client.Audiences)
                {
                    options.ProviderOptions.DefaultScopes.Add(audience);
                }

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
            services.AddScoped<IHowl, Howl>();
            services.AddScoped<IHowlGlobal, HowlGlobal>();

            return services;
        }
    }
}