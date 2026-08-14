using Karata.Kit;
using Karata.Kit.Bot.Interface;
using Karata.Kit.Bot.Services;
using Karata.Kit.Cards.Connection;
using Karata.Kit.Security;
using Karata.Runtime.Infrastructure;
using Karata.Runtime.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Karata.Runtime.Bot;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataBot(Uri host)
        {
            services.AddCors(cors => cors.AddPolicy(nameof(CrossOrigin.AllowAll), CrossOrigin.AllowAll));
            services.AddHttpClient();
            services.AddHealthChecks();
            services.AddMemoryCache();
            
            services.AddSingleton<IAccessTokenProvider, ClientCredentialsAccessTokenProvider>();
            services.AddKarataCards((cards, services) =>
            {
                cards.Host = host;
                cards.TokenProvider = async () => await services.GetRequiredService<IAccessTokenProvider>().GetAsync();
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
            
            await app.Services.GetRequiredService<PlayerConnection>().StartAsync();
        }
    }
}