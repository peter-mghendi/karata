using System;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Karata.App.Services;
using Karata.App.ViewModels;
using Karata.App.Views;
using Karata.Shared;
using Karata.Shared.Security;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace Karata.App.Infrastructure;

public static class CompositionRoot
{
    extension(IServiceCollection services)
    {
        public IServiceCollection Configure(ClientConfiguration configuration)
        {
            services.UseMicrosoftDependencyResolver();
        
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
        
            Locator.CurrentMutable.RegisterConstant<IActivationForViewFetcher>(new AvaloniaActivationForViewFetcher());
            Locator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new AutoDataTemplateBindingHook());
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        
            services.AddSingleton<IBrowser>(_ => new SystemBrowser(7890));
            services.AddSingleton(sp =>
            {
                var browser = sp.GetRequiredService<IBrowser>();
                return new OidcClient(new OidcClientOptions
                {
                    Authority = "http://localhost:8080/realms/karata",
                    ClientId = configuration.Authority,
                    Scope = "openid profile offline_access",
                    RedirectUri = "http://127.0.0.1:7890/",
                    PostLogoutRedirectUri = "http://127.0.0.1:7890/",
                    Browser = browser,
                    Policy = new Policy { RequireAccessTokenHash = false },
                    DisablePushedAuthorization = true
                });
            });
            services.AddSingleton<AuthService>();
            services.AddKarataCore(karata =>
            {
                karata.Host = new Uri("https://localhost:7240");
                karata.TokenProvider = async (sp, cancellation) =>
                {
                    var auth = sp.GetRequiredService<AuthService>();
                    return await auth.GetAccessTokenAsync(cancellation);
                };
            });
            
            services.AddSingleton<IScreen, ShellViewModel>();
        
            services.AddTransient<LoginViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<PlayViewModel>();
        
            services.AddTransient<IViewFor<LoginViewModel>, LoginView>();
            services.AddTransient<IViewFor<HomeViewModel>, HomeView>();
            services.AddTransient<IViewFor<PlayViewModel>, PlayView>();
        
            services.AddSingleton<IViewLocator, ServiceProviderViewLocator>();

            return services;
        }
    }
}
