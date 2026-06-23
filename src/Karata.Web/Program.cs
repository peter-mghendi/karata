using Karata.Kit.Application;
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
    .AddKarataCore((karata, services) =>
    {
        karata.Host = new Uri(Configuration.Server[builder.HostEnvironment.Environment].Host);
        karata.TokenProvider = async () => await TokenProvider.ProvideAsync(services);
    })
    .AddKarataSurface()
    .AddKarataBotInterface(new Uri(Configuration.BotInterface[builder.HostEnvironment.Environment].Host));

await builder.Build().RunAsync();