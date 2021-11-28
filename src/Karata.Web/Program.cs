#nullable enable

using System.Text.Json.Serialization;
using Blazored.Modal;
using Blazored.Toast;
using Karata.Web.Areas.Identity;
using Karata.Web.Data;
using Karata.Web.Engines;
using Karata.Web.Hubs;
using Karata.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<KarataContext>(options =>
{
    options.UseSqlite(connectionString);
    options.UseLazyLoadingProxies();
});
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<KarataContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR().AddHubOptions<GameHub>(options =>
{
    options.MaximumParallelInvocationsPerClient = 2;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
builder.Services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IEngine, KarataEngine>();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredToast();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "application/octet-stream" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapBlazorHub();
    endpoints.MapHub<GameHub>("/game");
    endpoints.MapFallbackToPage("/_Host");
});

app.Run();