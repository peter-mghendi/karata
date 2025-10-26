using System;
using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Karata.App.Infrastructure;
using Karata.App.ViewModels;
using Karata.App.Views;
using Microsoft.Extensions.Hosting;
using ReactiveUI;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace Karata.App;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var builder = new HostBuilder().ConfigureServices((_, services) => services.Configure());
        var host = builder.Build();
        host.Services.UseMicrosoftDependencyResolver();

        var screen = host.Services.GetService(typeof(IScreen)) as IScreen ??
                     throw new InvalidOperationException("IScreen not registered");
        var shell = new ShellView
        {
            ViewModel = screen as ShellViewModel ?? throw new InvalidOperationException("IScreen is not ShellViewModel")
        };

        var locator = (IViewLocator)host.Services.GetService(typeof(IViewLocator))!;
        shell.FindControl<RoutedViewHost>("RouterHost")!.ViewLocator = locator;

        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new Window { Content = shell, Title = "Karata" };
                desktop.Exit += (_, _) => host.Dispose();
                break;
            case ISingleViewApplicationLifetime singleView:
                singleView.MainView = shell;
                break;
        }

        RxApp.MainThreadScheduler.Schedule(() =>
        {
            var home = (HomeViewModel)host.Services.GetService(typeof(HomeViewModel))!;
            screen.Router.NavigateAndReset.Execute(home).Subscribe();
        });
        base.OnFrameworkInitializationCompleted();
    }
}