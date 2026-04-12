using System.Collections.Concurrent;
using Karata.Kit.Application.Client.State;
using Microsoft.AspNetCore.SignalR.Client;

namespace Karata.Kit.Application.Client.Connection;

public interface IUserConnection {
    Task StopAsync(CancellationToken ct = default);
    
    public interface ISessionParameters;

    public interface ISession : IDisposable
    {
        HubConnection? Hub { get; }
    }
}

public interface IUserConnection<in TStartParameters, out TSession> : IUserConnection 
    where TSession : IUserConnection.ISession
    where TStartParameters : IUserConnection.ISessionParameters 
{
    Task StartAsync(CancellationToken ct = default);

    public TSession Spawn(Guid roomId, TStartParameters parameters);
}