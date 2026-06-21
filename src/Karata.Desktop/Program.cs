
using Karata.Kit.Application;
using Karata.Kit.Bot;
using Karata.Surface;
using Karata.Surface.Security;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace Karata.Desktop;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);
        var environment = "Development"; 

        builder.Services.AddLogging();
        builder.RootComponents.Add<App>("#app");
        builder.Services
            .AddKarataOidc(Configuration.Client[environment])
            .AddKarataCore((karata, services) =>
            {
                karata.Host = new Uri(Configuration.Server[environment].Host);
                karata.TokenProvider = () => Task.FromResult(string.Empty)!;
                // karata.TokenProvider = async () => await TokenProvider.ProvideAsync(services);
            })
            .AddKarataSurface()
            .AddKarataBotInterface(new Uri(Configuration.BotInterface[environment].Host));
   
        var app = builder.Build();

        // customize window
        app.MainWindow
            // .SetIconFile("icon.png")
            .SetTitle("Karata Desktop");

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
        };

        app.Run();
    }
}