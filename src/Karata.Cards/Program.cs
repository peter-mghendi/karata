using Karata.Cards.Infrastructure;
using Karata.Cards.Routing;
using Karata.Cards.Services;
using Karata.Kit;
using Karata.Kit.Cards.Engine;
using Karata.Kit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using IncomingAccessTokenProvider = System.Func<System.Threading.Tasks.Task<string?>>;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set.");
var platform = builder.Configuration["PLATFORM_URL"] ?? throw new Exception("PLATFORM_URL is not set.");

builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddDatabase(db, builder.Environment);
builder.Services.AddMemoryCache();
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
builder.Services.AddSignalR();
builder.Services.AddSingleton<IKarataEngine, TwoPassKarataEngine>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSingleton<IPasswordService, Argon2PasswordService>();
builder.Services.AddSingleton<ReplayProcessor>();
builder.Services.AddTransient<CurrentUserService>();
builder.Services.AddTransient<GameStartServiceFactory>();
builder.Services.AddTransient<RoomMembershipServiceFactory>();
builder.Services.AddTransient<TurnProcessingServiceFactory>();
builder.Services.AddTransient<VoidTurnServiceFactory>();
builder.Services.AddTransient<SetAwayServiceFactory>();

builder.Services.AddKeyedScoped<IncomingAccessTokenProvider>(nameof(IncomingAccessTokenProvider), (sp, _) => async () =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    return await accessor.HttpContext!.GetTokenAsync("access_token");
});
builder.Services.AddTransient<TokenExchangeAccessTokenProvider>();
builder.Services.AddKarataPlatform((options, services) =>
{
    options.Host = new Uri(platform);
    options.TokenProvider = async () =>
    {
        using var scope = services.CreateScope();
        using var provider = scope.ServiceProvider.GetRequiredService<TokenExchangeAccessTokenProvider>();

        return await provider.GetAsync();
    };
});
builder.Services.AddResponseCompression(compression =>
{
    compression.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

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

app.MapHubs();
app.MapEndpoints();

app.Run();