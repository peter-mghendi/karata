using Karata.Kit.Bot.Strategy;
using Karata.Runtime;
using Karata.Runtime.Bot;
using Karata.Runtime.Bot.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var host = builder.Configuration["KARATA_CARDS_HOST"] ?? throw new Exception("KARATA_CARDS_HOST is not set");

Console.WriteLine($"Karata Host: {host}");

builder.Services.AddKarataBot(host: new Uri(host));
builder.Services.AddKeyedTransient<IBotStrategy, BailBotStrategy>(nameof(BailBotStrategy));
builder.Services.AddKeyedTransient<IBotStrategy, RandomValidBotStrategy>(nameof(RandomValidBotStrategy));

var app = builder.ConfigureRuntimeHost().Build();
await app.InitializeKarataBotAsync();

app.MapBotStrategy("bail", app.Services.GetRequiredKeyedService<IBotStrategy>(nameof(BailBotStrategy)));
app.MapBotStrategy("random", app.Services.GetRequiredKeyedService<IBotStrategy>(nameof(RandomValidBotStrategy)));
app.Run();