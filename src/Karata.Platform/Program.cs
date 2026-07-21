using System.Security.Claims;
using Karata.Platform;
using Karata.Platform.Data;
using Karata.Platform.Models;
using Karata.Runtime;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration["DATABASE_URL"] is {} url ? new Uri(url) : throw new Exception("DATABASE_URL is not set.");

builder.Services.AddKarataRuntime<PlatformContext, User>(
    configuration: builder.Configuration,
    configure: options =>
    {
        options.ConfigureDatabase(postgres => postgres.DataSource = db);
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
);

var app = builder.ConfigureRuntimeHost().Build();
await app.InitializeKarataRuntimeAsync<PlatformContext, User>();

app.MapEndpoints();
app.Run();  