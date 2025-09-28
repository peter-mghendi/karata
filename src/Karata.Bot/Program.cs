using Karata.Bot.Infrastructure.Security;
using Karata.Bot.Routes;
using Karata.Bot.Services;
using Karata.Shared.Engine;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddHttpClient("KarataClient", http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Karata:Host"]!);
    http.DefaultRequestHeaders.Add("Accept", "application/json");
    http.DefaultRequestHeaders.Add("User-Agent", "Karata.Bot/1.0");
});
builder.Services.AddSingleton<KeycloakAccessTokenProvider>();
builder.Services.AddSingleton<BotSessionFactory>();
builder.Services.AddSingleton<BotSessionManager>();
builder.Services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();

var app = builder.Build();
app.MapBotRoutes();
app.Run();