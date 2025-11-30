namespace Karata.Kit.Application;

public sealed class KarataClientOptions
{
    public required Uri Host { get; set; }

    public required Func<IServiceProvider, CancellationToken, Task<string?>> TokenProvider { get; set; }
}