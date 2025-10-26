using System;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Karata.App.ViewModels;
using Karata.App.Views;
using Karata.Shared;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace Karata.App.Infrastructure;

public static class CompositionRoot
{
    public static IServiceCollection Configure(this IServiceCollection services)
    {
        services.UseMicrosoftDependencyResolver();
        
        var resolver = Locator.CurrentMutable;
        resolver.InitializeSplat();
        resolver.InitializeReactiveUI();
        
        Locator.CurrentMutable.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
        Locator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        services.AddKarataCore(karata =>
        {
            karata.Host = new Uri("https://localhost:7240");
            karata.TokenProvider = async (_, _) => await Task.FromResult<string?>(string.Empty);
        });
            
        services.AddSingleton<IScreen, ShellViewModel>();
        
        services.AddTransient<HomeViewModel>();
        services.AddTransient<PlayViewModel>();

        services.AddTransient<IViewFor<HomeViewModel>, HomeView>();
        services.AddTransient<IViewFor<PlayViewModel>, PlayView>();
        
        services.AddSingleton<IViewLocator, ServiceProviderViewLocator>();

        return services;
    }
}
