using System.Security.Claims;
using Karata.Kit;
using Karata.Runtime;
using Karata.Runtime.Security;
using Karata.Trivia.Data;
using Karata.Trivia.Hubs;
using Karata.Trivia.Models;
using Karata.Trivia.Services;

var builder = WebApplication.CreateBuilder(args);

var db = builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set.");
var platform = builder.Configuration["PLATFORM_URL"] ?? throw new Exception("PLATFORM_URL is not set.");

builder.Services
    .AddKarataRuntime<TriviaContext, User>(
        configuration: builder.Configuration,
        configure: options =>
        {
            options.UseSignalR();
            options.UseTokenExchange();
            options.ConfigureDatabase(postgres => postgres.DataSource = new Uri(db));
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

builder.Services.AddControllers();
builder.Services.AddScoped<GameService>();

var app = builder.ConfigureRuntimeHost().Build();
await app.InitializeKarataRuntimeAsync<TriviaContext, User>();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification", options => options.AllowStatefulReconnects = true);
app.Run();