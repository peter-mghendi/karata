using Howler.Blazor.Components;
using Karata.Kit;
using Karata.Kit.Bot;
using Karata.Surface;
using Karata.Surface.Security;
using Karata.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
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
    .AddKarataSurface()
    .AddKarataBotInterface(new Uri(Configuration.Bot[builder.HostEnvironment.Environment].Host));

await builder.Build().RunAsync();