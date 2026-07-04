using CmdScale.EntityFrameworkCore.TimescaleDB;
using Karata.Platform.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Karata.Platform.Infrastructure;

public static class Database
{
    extension(IServiceCollection services)
    {
        public void AddDatabase(Uri uri, IWebHostEnvironment environment)
        {
            services.AddDbContext<KarataPlatformContext>(options =>
            {
                options.UseNpgsql(uri.ConnectionString).UseTimescaleDb();

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
            await scope.ServiceProvider.GetRequiredService<KarataPlatformContext>().Database.MigrateAsync();
        }
    }

    extension(Uri uri)
    {
        private string ConnectionString => uri.UserInfo.Split(':') switch
        {
            [var username, var password] => new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port,
                Username = username,
                Password = password,
                Database = uri.LocalPath.TrimStart('/'),
                SslMode = SslMode.Prefer
            }.ToString(),
            _ => throw new Exception("Invalid DATABASE_URL.")
        };
    }
}