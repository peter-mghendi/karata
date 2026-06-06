using Karata.Bot;
using Karata.BotFramework.Endpoints;
using Karata.Kit.Application;
using Karata.Kit.Bot;
using Karata.Kit.Bot.Infrastructure.Security;
using Karata.Kit.Bot.Strategy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(cors => cors.AddPolicy(nameof(CrossOrigin.AllowAll), CrossOrigin.AllowAll));
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<AccessTokenProvider>();
builder.Services.AddKarataCore((karata, services) =>
{
    karata.Host = new Uri(builder.Configuration["Karata:Host"]!);
    karata.TokenProvider = async () => await services.GetRequiredService<AccessTokenProvider>().GetAsync();
});

builder.Services.AddKarataBot();
builder.Services.AddKeyedTransient<IBotStrategy, BailBotStrategy>(nameof(BailBotStrategy));
builder.Services.AddKeyedTransient<IBotStrategy, RandomValidBotStrategy>(nameof(RandomValidBotStrategy));

var app = builder.Build();

await app.InitializeKarataBotAsync();

app.UseHttpsRedirection();
app.UseCors(nameof(CrossOrigin.AllowAll));

app.MapHealthChecks("/health");
app.MapBotStrategy("bail", app.Services.GetRequiredKeyedService<IBotStrategy>(nameof(BailBotStrategy)));
app.MapBotStrategy("random", app.Services.GetRequiredKeyedService<IBotStrategy>(nameof(RandomValidBotStrategy)));
app.Run();