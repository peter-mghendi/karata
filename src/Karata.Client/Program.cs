using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Karata.Client;
using Karata.Client.Infrastructure.Security;
using Karata.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor.Services;
using MudExtensions.Services;
using TextCopy;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

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
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.InjectClipboard();
builder.Services.AddKarataCore(karata =>
{
    karata.Host = new Uri(builder.HostEnvironment.BaseAddress);
    karata.TokenProvider = async (services, _) =>
    {
        var provider = services.GetRequiredService<IAccessTokenProvider>();
        var result = await provider.RequestAccessToken();

        return result.TryGetToken(out var token) ? token.Value : null;
    };
});
builder.Services.AddScoped<AuthenticationHelper>();

await builder.Build().RunAsync();
