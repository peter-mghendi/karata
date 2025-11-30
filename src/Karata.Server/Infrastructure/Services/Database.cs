using Karata.Server.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Karata.Server.Infrastructure.Services;

public static class Database
{
    extension(IServiceCollection services)
    {
        public void AddDatabase(string url, IWebHostEnvironment environment)
        {
            services.AddDbContext<KarataContext>(options =>
            {
                var uri = new Uri(url);
                if (uri.UserInfo.Split(':') is not [var username, var password])
                    throw new Exception("Invalid DATABASE_URL.");

                options.UseNpgsql(new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port,
                    Username = username,
                    Password = password,
                    Database = uri.LocalPath.TrimStart('/'),
                    SslMode = SslMode.Prefer
                }.ToString());

                if (environment.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            if (environment.IsDevelopment())
            {
                services.AddDatabaseDeveloperPageExceptionFilter();
            }
        }
    }
    extension(WebApplication app)
    {
        public async Task MaintainDatabaseAsync()
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<KarataContext>();

            await context.Database.MigrateAsync();
        }
    }
}