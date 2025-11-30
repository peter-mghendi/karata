using Blazored.LocalStorage;
using Karata.Kit.Application;
using Karata.Web;
using Karata.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudExtensions.Services;
using TextCopy;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOidcAuthentication(options =>
{
    var configuration = Configuration.Client[builder.HostEnvironment.Environment];

    // options.ProviderOptions.DefaultScopes.Add("Audience");
    options.ProviderOptions.Authority = configuration.Authority;
    options.ProviderOptions.ClientId = configuration.Client;
    options.ProviderOptions.MetadataUrl = $"{configuration.Authority}/.well-known/openid-configuration";
    options.ProviderOptions.ResponseType = "id_token token";
    options.UserOptions.NameClaim = "preferred_username";
    options.UserOptions.RoleClaim = "roles";
    options.UserOptions.ScopeClaim = "scope";
});
builder.Services.AddBlazoredLocalStorage();
builder.Services.InjectClipboard();
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.AddKarataCore(karata =>
{
    karata.Host = new Uri(builder.HostEnvironment.BaseAddress);
    karata.TokenProvider = async (services, _) =>
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IAccessTokenProvider>();
        var result = await provider.RequestAccessToken();

        return result.TryGetToken(out var token) ? token.Value : null;
    };
});
builder.Services.AddScoped<AuthenticationHelper>();

await builder.Build().RunAsync();
