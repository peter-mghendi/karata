using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Kit.Application.Client.Connection;

public interface IUserConnection {
    Task StartAsync(CancellationToken ct = default);

    Task StopAsync(CancellationToken ct = default);

    public interface ISessionParameters;

    public interface ISession : IDisposable
    {
        public HubConnection? Hub { get; }
    }
}

public interface IUserConnection<in TStartParameters, out TSession> : IUserConnection
    where TStartParameters : IUserConnection.ISessionParameters 
    where TSession : IUserConnection.ISession
{
    public TSession Spawn(Guid roomId, TStartParameters parameters);
}