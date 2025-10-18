using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Karata.App.ViewModels;
using Karata.App.Views;
using Karata.Shared.Client;
using MainView = Karata.App.Views.MainView;
using MainWindow = Karata.App.Views.MainWindow;

namespace Karata.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var karata = new KarataClient(new Uri("https://localhost:7240"), () => Task.FromResult<string?>(string.Empty));
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(karata)
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(karata)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

}