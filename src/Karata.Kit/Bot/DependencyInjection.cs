using Karata.Kit.Bot.Interface;
using Karata.Kit.Bot.Services;
using Karata.Kit.Cards.Connection;
using Karata.Kit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Karata.Kit.Bot;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataBot()
        {
            services.AddSingleton<PlayerConnection>(provider =>
            {
                var tokens = provider.GetRequiredService<ClientCredentialsAccessTokenProvider>();
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

        public IServiceCollection AddKarataBotInterface(Uri host)
        {
            services.AddSingleton<BotInterface>(_ => new BotInterface(host));
            return services;
        }
    }

    extension(IHost host)
    {
        public async Task InitializeKarataBotAsync()
        {
            await host.Services.GetRequiredService<PlayerConnection>().StartAsync();
        }
    }
}