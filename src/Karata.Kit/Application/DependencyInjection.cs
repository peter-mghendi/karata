using Karata.Kit.Application.Client;
using Karata.Kit.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Karata.Kit.Application;

public static class DependencyInjection 
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataCore(Action<KarataClientOptions, IServiceProvider> configure)
        {
            services.AddOptions<KarataClientOptions>().Configure(configure);
            services.AddSingleton<KarataClient>(sp => new(options: sp.GetRequiredService<IOptions<KarataClientOptions>>().Value));
            services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
            return services;
        }
    }
}