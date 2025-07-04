using Karata.Pebble.StateActions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Karata.Pebble.Interceptors;

public abstract class Interceptor<TState>
{
    protected static ILoggerFactory DefaultLoggerFactory => NullLoggerFactory.Instance;

    public delegate TState Reducer(TState state, StateAction<TState> action);

    public abstract Reducer Wrap(Reducer next);
}