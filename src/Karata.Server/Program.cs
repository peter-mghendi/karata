using Karata.Kit.Engine;
using Karata.Server.Endpoints;
using Karata.Server.Infrastructure.Security;
using Karata.Server.Infrastructure.Services;
using Karata.Server.Services;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set.");

builder.Services.AddDatabase(db, builder.Environment);
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
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
builder.Services.AddSignalR();
builder.Services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<IPasswordService, Argon2PasswordService>();
builder.Services.AddTransient<CurrentUserService>();
builder.Services.AddTransient<GameStartServiceFactory>();
builder.Services.AddTransient<RoomMembershipServiceFactory>();
builder.Services.AddTransient<TurnProcessingServiceFactory>();
builder.Services.AddTransient<VoidTurnServiceFactory>();
builder.Services.AddTransient<SetAwayServiceFactory>();
builder.Services.AddResponseCompression(compression =>
{
    compression.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

// builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages();

if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
{
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
}

var app = builder.Build();
await app.MaintainDatabaseAsync();

app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
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

app.MapHubs();
app.MapEndpoints();
app.MapStaticAssets();
app.MapFallbackToFile("index.html");

app.Run();