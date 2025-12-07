using Karata.Kit.Bot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Karata.Kit.Bot;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataBot()
        {
            services.AddSingleton<BotSessionFactory>();
            services.AddSingleton<BotSessionManager>();
            
            return services;
        }
    }
}