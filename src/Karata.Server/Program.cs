using Karata.Server.Data;
using Karata.Server.Hubs;
using Karata.Server.Services;
using Karata.Shared.Engine;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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

builder.Services
    .AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<KarataContext>();
builder.Services.AddIdentityServer()
    .AddApiAuthorization<User, KarataContext>();
builder.Services.AddAuthentication()
    .AddIdentityServerJwt();
builder.Services.TryAddEnumerable(
    ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>()
);

builder.Services.AddSignalR();
builder.Services.AddSingleton<KarataEngine>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<IPasswordService, Argon2PasswordService>();
builder.Services.AddTransient<GameStartServiceFactory>();
builder.Services.AddTransient<RoomMembershipServiceFactory>();
builder.Services.AddTransient<TurnProcessingServiceFactory>();
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
var context = services.GetRequiredService<DbContext>();
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

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<PlayerHub>("/hubs/game/play", options => options.AllowStatefulReconnects = true);
app.MapHub<SpectatorHub>("/hubs/game/spectate", options => options.AllowStatefulReconnects = true);
app.MapFallbackToFile("index.html");

app.Run();