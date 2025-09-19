using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Services;
using Karata.Shared.Engine;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<KarataContext>(options =>
{
    var uri = new Uri(builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set."));
    var credentials = uri.UserInfo.Split(':');
    
    options.UseNpgsql(new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = credentials.First(),
        Password = credentials.Last(),
        Database = uri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Prefer
    }.ToString());

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
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

builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<IPasswordService, Argon2PasswordService>();
builder.Services.AddTransient<GameStartServiceFactory>();
builder.Services.AddTransient<RoomMembershipServiceFactory>();
builder.Services.AddTransient<TurnProcessingServiceFactory>();
builder.Services.AddTransient<VoidTurnServiceFactory>();
builder.Services.AddTransient<SetAwayServiceFactory>();
builder.Services.AddResponseCompression(compression =>
{
    compression.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
{
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
}

var app = builder.Build();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<KarataContext>();
await context.Database.MigrateAsync();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<PlayerHub>("/hubs/game/play", options => options.AllowStatefulReconnects = true);
app.MapHub<SpectatorHub>("/hubs/game/spectate", options => options.AllowStatefulReconnects = true);
app.MapFallbackToFile("index.html");

app.Run();