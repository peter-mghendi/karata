using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames.Application;
using static System.Net.Mime.MediaTypeNames.Text;

namespace Karata.App.Services;

public class SystemBrowser(int? port = null, string path = "") : IBrowser
{
    private int Port { get; } = port ?? GetRandomUnusedPort();

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellation = default)
    {
        using var listener = new LoopbackHttpListener(Port, path);
        OpenBrowser(options.StartUrl);

        try
        {
            return (await listener.WaitForCallbackAsync()).Trim() is [_, ..]
                ? new BrowserResult { Response = await listener.WaitForCallbackAsync(), ResultType = BrowserResultType.Success }
                : new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
        }
        catch (TaskCanceledException ex)
        {
            return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
        }
        catch (Exception ex)
        {
            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
        }
    }

    private static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
    }

    private static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}

file sealed class LoopbackHttpListener : IDisposable
{
    private const int DefaultTimeoutInSeconds = checked((int)(5 * TimeSpan.SecondsPerMinute));

    private readonly IHost _host;
    private readonly TaskCompletionSource<string> _source = new();
    private readonly string _url;

    public string Url => _url;

    public LoopbackHttpListener(int port, string? path = "")
    {
        if (path is ['/', .. var trimmed]) path = trimmed;

        _url = $"http://127.0.0.1:{port}/{path}"; 
        _host = new HostBuilder()
            .ConfigureWebHost(builder => builder.UseKestrel().UseUrls(_url).Configure(Configure))
            .Build();

        _host.Start();
    }

    public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeoutInSeconds)
    {
        Task.Run(async () =>
        {
            await Task.Delay(timeoutInSeconds * 1000);
            _source.TrySetCanceled();
        });

        return _source.Task;
    }

    public void Dispose()
    {
        Task.Run(async () =>
        {
            await Task.Delay(500);
            _host.Dispose();
        });
    }

    private void Configure(IApplicationBuilder app)
    {
        app.Run(async ctx =>
        {
            switch (ctx.Request.Method)
            {
                case "GET":
                    await SetResultAsync(ctx.Request.QueryString.Value!, ctx);
                    break;
                case "POST" when !ctx.Request.ContentType!.Equals(FormUrlEncoded, StringComparison.OrdinalIgnoreCase):
                    ctx.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                    break;
                case "POST":
                {
                    using var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8);
                    var body = await sr.ReadToEndAsync();
                    await SetResultAsync(body, ctx);

                    break;
                }
                default:
                    ctx.Response.StatusCode = 405;
                    break;
            }
        });
    }

    private async Task SetResultAsync(string value, HttpContext ctx)
    {
        try
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            ctx.Response.ContentType = "text/html";
            await ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
            await ctx.Response.Body.FlushAsync();

            _source.TrySetResult(value);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());

            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            ctx.Response.ContentType = Html;
            await ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
            await ctx.Response.Body.FlushAsync();
        }
    }
}