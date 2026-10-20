namespace Karata.Kit.Support.Http;

// TODO [Legacy]: There's no reason for this to be an interface. Clean up injection sites and merge implementations.
public interface IClientOptions
{
    public Uri Host { get; }

    public Func<Task<string?>> TokenProvider { get; }
}