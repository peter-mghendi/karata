using System.Security.Claims;
using Karata.Cards;
using Karata.Cards.Data;
using Karata.Cards.Services;
using Karata.Kit;
using Karata.Kit.Cards.Engine;
using Karata.Runtime;
using Karata.Runtime.Security;

var builder = WebApplication.CreateBuilder(args);

var db = builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set.");
var platform = builder.Configuration["PLATFORM_URL"] ?? throw new Exception("PLATFORM_URL is not set.");

builder.Services
    .AddKarataRuntime<CardsContext, User>(
        configuration: builder.Configuration,
        configure: options =>
        {
            options.UseSignalR();
            options.UseTokenExchange();
            options.ConfigureDatabase((postgres) => postgres.DataSource = new Uri(db));
            options.ConfigureUserProvisioning(provisioning =>
            {
                provisioning.Enabled = true;
                provisioning.Factory = principal => new User
                {
                    Id = principal.FindFirstValue(ClaimTypes.NameIdentifier)!,
                    // Email = principal.FindFirstValue(ClaimTypes.Email)!,
                    Username = principal.FindFirstValue("preferred_username")!
                };
            });
        }
    )
    .AddKarataPlatform((options, services) =>
    {
        options.Host = new Uri(platform);
        options.TokenProvider = async () =>
        {
            using var scope = services.CreateScope();
            using var provider = scope.ServiceProvider.GetRequiredService<TokenExchangeAccessTokenProvider>();

            return await provider.GetAsync();
        };
    });

builder.Services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<ReplayProcessor>();
builder.Services.AddTransient<GameStartServiceFactory>();
builder.Services.AddTransient<RoomMembershipServiceFactory>();
builder.Services.AddTransient<TurnProcessingServiceFactory>();
builder.Services.AddTransient<VoidTurnServiceFactory>();
builder.Services.AddTransient<SetAwayServiceFactory>();

Console.WriteLine($"Karata.Cards is running in {builder.Environment.EnvironmentName} mode.");
builder.Services.AddKeyedSingleton(nameof(Configuration.Web), Configuration.Web[builder.Environment.EnvironmentName]);

var app = builder.ConfigureRuntimeHost().Build();
await app.InitializeKarataRuntimeAsync<CardsContext, User>();

app.MapHubs();
app.MapEndpoints();
app.Run();