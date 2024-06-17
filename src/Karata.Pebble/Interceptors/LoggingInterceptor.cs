using static System.Console;

namespace Karata.Pebble.Interceptors;

public class LoggingInterceptor<TState> : Interceptor<TState>
{
    public override void BeforeChange(TState state) => WriteLine($"Before Change: State={state}");

    public override void AfterChange(TState state) => WriteLine($"After Change: State={state}");
}