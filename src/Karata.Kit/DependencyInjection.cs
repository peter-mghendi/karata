using Karata.Kit.Cards.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Karata.Kit;

public static class DependencyInjection 
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataCards(Action<Cards.Client.Options, IServiceProvider> configure)
        {
            services.AddOptions<Cards.Client.Options>().Configure(configure);
            services.AddSingleton<Cards.Client>(sp => new(options: sp.GetRequiredService<IOptions<Cards.Client.Options>>().Value));
            services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
            return services;
        }

        public IServiceCollection AddKarataPlatform(Action<Platform.Client.Options, IServiceProvider> configure)
        {
            services.AddOptions<Platform.Client.Options>().Configure(configure);
            services.AddSingleton<Platform.Client>(sp => new(options: sp.GetRequiredService<IOptions<Platform.Client.Options>>().Value));
            return services;
        }

        public IServiceCollection AddKarataTrivia(Action<Trivia.Client.Options, IServiceProvider> configure)
        {
            services.AddOptions<Trivia.Client.Options>().Configure(configure);
            services.AddSingleton<Trivia.Client>(sp => new(options: sp.GetRequiredService<IOptions<Trivia.Client.Options>>().Value));
            return services;
        }
    }
}