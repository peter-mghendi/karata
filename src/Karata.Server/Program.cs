using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.ResponseCompression;
using Karata.Server.Data;
using Npgsql;
using Microsoft.AspNetCore.SignalR;
using Karata.Server.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Karata.Server.Engines;
using Microsoft.EntityFrameworkCore;
using Karata.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);


// TODO: Standardize this. Use the URL format for development too, to avoid this check.
string connectionString;
if (builder.Environment.IsDevelopment())
    connectionString = builder.Configuration["DefaultConnection"] ?? throw new Exception("DefaultConnection is empty. Have you set it in user secrets?");
else
{
    var databaseUri = new Uri(builder.Configuration["DATABASE_URL"] ?? throw new Exception("DATABASE_URL is not set. Have you set it in the environment?"));
    var userInfo = databaseUri.UserInfo.Split(':');
    connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Prefer,
        TrustServerCertificate = true
    }.ToString();
}

builder.Services.AddDbContext<KarataContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseLazyLoadingProxies();
});
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<KarataContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSignalR().AddHubOptions<GameHub>(options =>
{
    options.MaximumParallelInvocationsPerClient = 2;
});
// .AddJsonProtocol(options =>
// {
//     options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// });

builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IEngine, KarataEngine>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/octet-stream" });
});

builder.Services.AddIdentityServer()
    .AddApiAuthorization<User, KarataContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// For running in Railway
var portString = Environment.GetEnvironmentVariable("PORT");
if (portString is {Length: > 0} && int.TryParse(portString, out var port))
{
    builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
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
app.MapHub<GameHub>("/game");
app.MapFallbackToFile("index.html");

app.Run();
