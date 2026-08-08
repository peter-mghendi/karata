using Karata.Bot;
using Karata.BotFramework.Endpoints;
using Karata.Kit;
using Karata.Kit.Bot;
using Karata.Kit.Bot.Strategy;
using Karata.Kit.Security;

var builder = WebApplication.CreateBuilder(args);
var host = builder.Configuration["KARATA_CARDS_HOST"] ?? throw new Exception("KARATA_CARDS_HOST is not set");

Console.WriteLine($"Karata Host: {host}");

builder.Services.AddCors(cors => cors.AddPolicy(nameof(CrossOrigin.AllowAll), CrossOrigin.AllowAll));
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ClientCredentialsAccessTokenProvider>();
builder.Services.AddKarataCards((cards, services) =>
{
    cards.Host = new Uri(host!);
    cards.TokenProvider = async () => await services.GetRequiredService<ClientCredentialsAccessTokenProvider>().GetAsync();
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