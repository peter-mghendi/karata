using Karata.Go.Data;
using Microsoft.EntityFrameworkCore;

namespace Karata.Go.Services;

public class LinkCleanupService(IServiceScopeFactory factory, ILogger<LinkCleanupService> logger) : BackgroundService, IDisposable
{
    private static readonly TimeSpan CooldownPeriod = TimeSpan.FromDays(30);
    private static readonly TimeSpan TimerPeriod = TimeSpan.FromDays(1);

    private readonly PeriodicTimer _timer = new(TimerPeriod);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var count = await CleanupAsync(cutoff: DateTimeOffset.UtcNow - CooldownPeriod, stoppingToken);
                logger.LogInformation("Link cleanup completed. {Count} links deleted.", count);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Link cleanup failed.");
            }
        }
    }

    private async Task<int> CleanupAsync(DateTimeOffset cutoff, CancellationToken stoppingToken)
    {
        await using var scope = factory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GoContext>();
        
        return await db.Links.Where(link => link.ExpiredAt < cutoff).ExecuteDeleteAsync(stoppingToken);
    }
    
    public override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }
}