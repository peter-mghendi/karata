using Karata.Kit.Platform.Models;
using Karata.Platform.Data;
using Karata.Platform.Infrastructure;
using Karata.Platform.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.TypedResults;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set.");

builder.Services.AddOpenApi();
builder.Services.AddDatabase(db, builder.Environment);
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.Configure<UserProvisioningOptions>(o => o.AutoProvisionEnabled = true);
builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opts =>
{
    var @base = opts.Events.OnMessageReceived;
    opts.Events.OnMessageReceived = async ctx =>
    {
        await @base(ctx);

        if (!ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/game")) return;
        if (ctx.Request.Query["access_token"] is not [_, ..] token) return;

        ctx.Token = token;
    };
});

builder.Services.AddCors(cors => cors.AddPolicy(nameof(CrossOrigin.AllowAll), CrossOrigin.AllowAll));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
builder.Services.AddTransient<CurrentUserService>();
builder.Services.AddTransient<IClaimsTransformation, UserProvisioningClaimsTransformation>();

if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
{
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
}

var app = builder.Build();
await app.MaintainDatabaseAsync();

app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(nameof(CrossOrigin.AllowAll));

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
api.MapGet(
        "/profiles",
        async ([FromServices] KarataPlatformContext context) =>
        {
            var profiles = await context.Users.AsNoTracking().ToArrayAsync();
            return Ok(profiles.Select(profile => new ProfileData(profile.Id, profile.Username, $"https://api.dicebear.com/10.x/glyphs/svg?seed={profile.Username}")));
        })
    .WithName("ListProfiles")
    .RequireAuthorization();
api.MapGet(
        "/profiles/{username}",
        async Task<Results<Ok<ProfileData>, NotFound>> ([FromServices] KarataPlatformContext context, string username) =>
        {
            var profile = await context.Users.AsNoTracking().FirstOrDefaultAsync(profile => profile.Username == username);
            if (profile is null) return NotFound();
            
            return Ok(new ProfileData(profile.Id, profile.Username, $"https://api.dicebear.com/10.x/glyphs/svg?seed={profile.Username}"));
        })
    .WithName("GetProfile")
    .RequireAuthorization();

app.UseHttpsRedirection();
app.Run();  