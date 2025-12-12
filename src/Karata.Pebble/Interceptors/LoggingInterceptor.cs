using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Karata.Pebble.Interceptors;

public class LoggingInterceptor<TState>(ILoggerFactory? factory = null) : Interceptor<TState>
{
    private readonly ILogger _logger = (factory ?? DefaultLoggerFactory).CreateLogger<LoggingInterceptor<TState>>();
    
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };
    
    public override Reducer Wrap(Reducer next) => (state, action) =>
    {
        _logger.LogDebug("Action: {Action}", action.ToString());
        _logger.LogDebug("Before: {State}", JsonSerializer.Serialize(state, _serializerOptions));

        var updated = next(state, action);
        
        _logger.LogDebug("After: {State}", JsonSerializer.Serialize(updated, _serializerOptions));
        return updated;
    };
}