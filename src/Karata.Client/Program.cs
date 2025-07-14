using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Karata.Client;
using Karata.Client.Infrastructure.State;
using MudBlazor.Services;
using MudExtensions.Services;
using TextCopy;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var host = builder.HostEnvironment.BaseAddress;
builder.Services
    .AddHttpClient("Karata.Server", client => client.BaseAddress = new Uri(host))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddHttpClient("Karata.Server.Public", client => client.BaseAddress = new Uri(host));

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Karata.Server"));
builder.Services.AddKeyedScoped<HttpClient>(
    "Karata.Server.Public", 
    (sp, _) => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Karata.Server.Public")
);

builder.Services.AddSingleton<RoomStoreFactory>();

builder.Services.AddApiAuthorization();
builder.Services.AddMudServices();
builder.Services.AddMudExtensions();
builder.Services.InjectClipboard();

await builder.Build().RunAsync();
