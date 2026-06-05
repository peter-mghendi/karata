using Karata.Kit.Application.Client.Connection;
using Karata.Kit.Bot.Infrastructure.Security;
using Karata.Kit.Bot.Interface;
using Karata.Kit.Bot.Services;
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
                var tokens = provider.GetRequiredService<AccessTokenProvider>();
                var config = provider.GetRequiredService<IConfiguration>();
                
                return new PlayerConnection(new Uri(config["Karata:Host"]!))
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