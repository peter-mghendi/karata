using Karata.Platform;
using Karata.Platform.Infrastructure;
using Karata.Platform.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
var db = builder.Configuration["DATABASE_URL"] is {} url ? new Uri(url) : throw new Exception("DATABASE_URL is not set.");

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

app.MapEndpoints();

app.UseHttpsRedirection();
app.Run();  