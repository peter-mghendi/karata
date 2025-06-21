using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Karata.Pebble.Interceptors;

public class TimingInterceptor<TState>(ILoggerFactory? logging = null) : Interceptor<TState>
{
    private readonly ILogger _logger = (logging ?? DefaultLoggerFactory).CreateLogger<LoggingInterceptor<TState>>();

    public override Reducer Wrap(Reducer next) => (state, action) =>
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var updated = next(state, action);
        
        stopwatch.Stop();
        _logger.LogDebug("Action {Action} took {Milliseconds} ms.", action.GetType().Name, stopwatch.ElapsedMilliseconds);
        
        return updated;
    };
}