using Karata.Runtime.Data;
using Karata.Runtime.Infrastructure;
using Karata.Runtime.Models;
using Karata.Runtime.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Karata.Runtime;

public static class DependencyInjection
{
    public sealed class Options<TUser> where TUser : KarataUser
    {
        internal bool IsSignalREnabled { get; private set; }
        internal bool IsTokenExchangeEnabled { get; private set; }
        internal Action<DatabaseOptions> DatabaseConfiguration { get; private set; } = _ => { };
        internal Action<UserProvisioningOptions<TUser>> UserProvisioningConfiguration { get; private set; } = _ => { };

        public void UseSignalR(bool use = true) => IsSignalREnabled = use;
        
        public void UseTokenExchange(bool use = true) => IsTokenExchangeEnabled = use;

        public void ConfigureDatabase(Action<DatabaseOptions> configure) => DatabaseConfiguration = configure;

        public void ConfigureUserProvisioning(Action<UserProvisioningOptions<TUser>> configure) => UserProvisioningConfiguration = configure;
    }
    
    extension(IServiceCollection services)
    {
        public IServiceCollection AddKarataRuntime<TContext, TUser>(IConfiguration configuration, Action<Options<TUser>> configure)
            where TContext : KarataContext<TUser>
            where TUser : KarataUser
        {
            var compress = ResponseCompressionDefaults.MimeTypes;
            var runtime = new Options<TUser>();
            configure(runtime);
                
            services.Configure(runtime.DatabaseConfiguration);
            services.Configure(runtime.UserProvisioningConfiguration);

            services.AddOpenApi();
            services.AddDatabase<TContext, TUser>();
            services.AddMemoryCache();
            services.AddKeycloakWebApiAuthentication(configuration);
            services.AddAuthorization();

            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                var @base = opts.Events.OnMessageReceived;
                opts.Events.OnMessageReceived = async ctx =>
                {
                    await @base(ctx);

                    if (!ctx.HttpContext.Request.Path.StartsWithSegments("/hubs")) return;
                    if (ctx.Request.Query["access_token"] is not [_, ..] token) return;

                    ctx.Token = token;
                };
            });

            services.AddCors(cors => cors.AddPolicy(nameof(CrossOrigin.AllowAll), CrossOrigin.AllowAll));

            services.AddHealthChecks();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            if (runtime.IsSignalREnabled)
            {
                services.AddSignalR();
                compress = compress.Concat(["application/octet-stream"]);
            }

            services.AddResponseCompression(compression => compression.MimeTypes = compress);

            services.AddTransient<CurrentUserService<TContext, TUser>>();
            services.AddTransient<IClaimsTransformation, UserProvisioningClaimsTransformation<TContext, TUser>>();

            if (runtime.IsTokenExchangeEnabled)
            {
                services.AddTransient<TokenExchangeAccessTokenProvider>();
            }

            return services;
        }
    }
    
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder ConfigureRuntimeHost()
        {
            if (int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port))
            {
                builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
            }
            return builder;
        }
    }

    extension(WebApplication app)
    {
        public async Task InitializeKarataRuntimeAsync<TContext, TUser>()
            where TContext : KarataContext<TUser>
            where TUser : KarataUser
        {
            await app.MaintainDatabaseAsync<TContext, TUser>();

            app.UseForwardedHeaders(new() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
            app.UseResponseCompression();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(nameof(CrossOrigin.AllowAll));

            app.MapHealthChecks("/health");

            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}