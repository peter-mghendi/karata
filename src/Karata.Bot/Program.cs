using Karata.BotFramework.Endpoints;
using Karata.Kit.Application;
using Karata.Kit.Bot;
using Karata.Kit.Bot.Infrastructure.Security;
using Karata.Kit.Bot.Strategy;

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

builder.Services.AddKarataBot();
builder.Services.AddKeyedTransient<IBotStrategy, BailBotStrategy>(nameof(BailBotStrategy));
builder.Services.AddKeyedTransient<IBotStrategy, RandomValidBotStrategy>(nameof(RandomValidBotStrategy));

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapBotStrategy("bail", app.Services.GetRequiredKeyedService<IBotStrategy>(nameof(BailBotStrategy)));
app.MapBotStrategy("random", app.Services.GetRequiredKeyedService<IBotStrategy>(nameof(RandomValidBotStrategy)));
app.Run();