using Karata.Kit;
using Karata.Kit.Cards.Connection;
using Karata.Kit.Platform;
using Karata.Runtime.Bot.Services;
using Karata.Runtime.Infrastructure;
using Karata.Runtime.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Runtime.Bot;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataBot()
        {
            services.AddCors(cors => cors.AddPolicy(nameof(CrossOrigin.AllowAll), CrossOrigin.AllowAll));
            services.AddHttpClient();
            services.AddHealthChecks();
            services.AddMemoryCache();
            
            services.AddSingleton<IAccessTokenProvider, ClientCredentialsAccessTokenProvider>();
            services.AddKarataCards((cards, provider) =>
            {
                cards.Host = new Uri(provider.GetRequiredService<IConfiguration>()["KARATA_CARDS_HOST"]!);
                cards.TokenProvider = async () => await provider.GetRequiredService<IAccessTokenProvider>().GetAsync();
            });
            services.AddKarataPlatform((platform, provider) =>
            {
                platform.Host = new Uri(provider.GetRequiredService<IConfiguration>()["KARATA_PLATFORM_HOST"]!);
                platform.TokenProvider = async () => await provider.GetRequiredService<IAccessTokenProvider>().GetAsync();
            });
            services.AddSingleton<PlayerConnection>(provider =>
            {
                var tokens = provider.GetRequiredService<IAccessTokenProvider>();
                var config = provider.GetRequiredService<IConfiguration>();
                
                return new PlayerConnection(new Uri(config["KARATA_CARDS_HOST"]!))
                {
                    AccessTokenProvider = async () => await tokens.GetAsync()
                };
            });
            
            services.AddSingleton<BotSessionFactory>();
            services.AddSingleton<BotSessionManager>();
            return services;
        }
    }

    extension(WebApplication app)
    {
        public async Task InitializeKarataBotAsync()
        {
            app.UseHttpsRedirection();
            app.UseCors(nameof(CrossOrigin.AllowAll));
            app.MapHealthChecks("/health");

            await app.Services.GetRequiredService<Client>().Profiles.GetAsync("service-account-karata-bot");
            await app.Services.GetRequiredService<PlayerConnection>().StartAsync();
        }
    }
}