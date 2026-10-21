using Karata.Kit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Karata.Operations;
using Karata.Surface;
using Karata.Surface.Security;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddKeyedSingleton(nameof(Configuration.Web), Configuration.Web[builder.HostEnvironment.Environment]);
builder.Services
    .AddKarataOidc(Configuration.Client[builder.HostEnvironment.Environment])
    .AddKarataCards((cards, services) =>
    {
        cards.Host = new Uri(Configuration.Cards[builder.HostEnvironment.Environment].Host);
        cards.TokenProvider = async () => await TokenProvider.ProvideAsync(services);
    })
    .AddKarataPlatform((platform, services) =>
    {
        platform.Host = new Uri(Configuration.Platform[builder.HostEnvironment.Environment].Host);
        platform.TokenProvider = async () => await TokenProvider.ProvideAsync(services);
    })
    .AddKarataTrivia((trivia, services) =>
    {
        trivia.Host = new Uri(Configuration.Trivia[builder.HostEnvironment.Environment].Host);
        trivia.TokenProvider = async () => await TokenProvider.ProvideAsync(services);
    })
    .AddKarataSurface();

await builder.Build().RunAsync();