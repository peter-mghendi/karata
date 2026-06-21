using Karata.Kit.Application;
using Karata.Kit.Bot;
using Karata.Surface;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;
using Photino.NET;

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
                
                // TODO: [Desktop] Investigate WebViewNavigationManager bug blocking desktop auth
                // karata.TokenProvider = async () => await TokenProvider.ProvideAsync(services);
            })
            .AddKarataSurface()
            .AddKarataBotInterface(new Uri(Configuration.BotInterface[environment].Host));

        var app = builder.Build();
        app.MainWindow
            // TODO: [Desktop] Use icon without compressed PNG layers for compatibility
            // .SetIconFile("favicon.ico")
            .SetTitle("Karata Desktop");

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
        };

        Task.Run(() => app.MainWindow.ShowDisclaimer());
        app.Run();
    }
}

file static class Disclaimer
{
    extension(PhotinoWindow window)
    {
        public void ShowDisclaimer()
        {
            window.ShowMessage(
                "Welcome to Karata Desktop!",
                "This application is still under development, and, as such, may lack some features."
            );
        }
    }
}