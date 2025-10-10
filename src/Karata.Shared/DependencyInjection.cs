using Karata.Shared.Client;
using Karata.Shared.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Karata.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddKarataCore(this IServiceCollection services, Action<KarataClientOptions> configure)
    {
        services.AddOptions<KarataClientOptions>().Configure(configure);
        services.AddSingleton<KarataClient>(sp =>
        {
            var o = sp.GetRequiredService<IOptions<KarataClientOptions>>().Value;
            return new KarataClient(o.Host, () => o.TokenProvider(sp, default));
        });

        services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
        return services;
    }
}