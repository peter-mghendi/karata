using Karata.Bot.Infrastructure.Security;
using Karata.Bot.Routes;
using Karata.Bot.Services;
using Karata.Kit.Application;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AccessTokenProvider>();
builder.Services.AddKarataCore(karata =>
{
    karata.Host = new Uri(builder.Configuration["Karata:Host"]!);
    karata.TokenProvider = async (services, cancellation) =>
    {
        var provider = services.GetRequiredService<AccessTokenProvider>();
        return await provider.GetAccessTokenAsync(cancellation);
    };
});

builder.Services.AddSingleton<BotSessionFactory>();
builder.Services.AddSingleton<BotSessionManager>();

var app = builder.Build();
app.MapHealthChecks("/health");
app.MapBotRoutes();
app.Run();