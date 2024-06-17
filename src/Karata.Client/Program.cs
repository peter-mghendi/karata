using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Karata.Client;
using Karata.Client.Infrastructure.State;
using MudBlazor.Services;
using TextCopy;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Karata.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Karata.ServerAPI"));
builder.Services.AddSingleton<RoomStoreFactory>();

builder.Services.AddApiAuthorization();
builder.Services.AddMudServices();
builder.Services.InjectClipboard();

await builder.Build().RunAsync();
