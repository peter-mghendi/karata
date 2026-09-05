using System.Security.Claims;
using Karata.Go;
using Karata.Go.Data;
using Karata.Go.Models;
using Karata.Go.Services;
using Karata.Runtime;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration["DATABASE_URL"] is { } url
    ? new Uri(url)
    : throw new Exception("DATABASE_URL is not set.");

builder.Services
    .AddKarataRuntime<GoContext, User>(
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
    )
    .AddHostedService<LinkCleanupService>();

var app = builder.ConfigureRuntimeHost().Build();
await app.InitializeKarataRuntimeAsync<GoContext, User>();

app.MapEndpoints();
app.Run();