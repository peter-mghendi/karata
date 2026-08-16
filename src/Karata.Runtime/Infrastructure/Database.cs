using CmdScale.EntityFrameworkCore.TimescaleDB;
using Karata.Runtime.Data;
using Karata.Runtime.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Karata.Runtime.Infrastructure;

public static class Database
{
    extension(IServiceCollection services)
    {
        public void AddDatabase<TContext, TUser>()
            where TContext : KarataContext<TUser>
            where TUser : KarataUser
        {
            
            services.AddDbContext<TContext>((sp, options) =>
            {
                var config = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                var env = sp.GetRequiredService<IWebHostEnvironment>();

                if (config.DataSource.UserInfo.Split(':') is not [var username, var password])
                    throw new Exception("Invalid DATABASE_URL.");

                var connection = new NpgsqlConnectionStringBuilder
                {
                    Host = config.DataSource.Host,
                    Port = config.DataSource.Port,
                    Username = username,
                    Password = password,
                    Database = config.DataSource.LocalPath.TrimStart('/'),
                    SslMode = SslMode.Prefer
                };
                options.UseNpgsql(connection.ToString()).UseTimescaleDb();

                if (env.IsDevelopment())
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            services.AddSingleton<IStartupFilter, DeveloperExceptionPageFilter>();
        }
    }

    extension(WebApplication app)
    {
        public async Task MaintainDatabaseAsync<TContext, TUser>() 
            where TContext : KarataContext<TUser>
            where TUser : KarataUser
        {
            using var scope = app.Services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TContext>();
            await context.Database.MigrateAsync();
        }
    }
}

file sealed class DeveloperExceptionPageFilter(IWebHostEnvironment env) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
        
        next(app);
    };
}