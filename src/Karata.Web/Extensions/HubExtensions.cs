using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Web.Extensions;

public static class HubExtensions
{
    /** 
     *  TODO: Timeouts and error handling.
     *
     *  REF: https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-wrap-eap-patterns-in-a-task
     *  REF: https://devblogs.microsoft.com/premier-developer/the-danger-of-the-taskcompletionsourcet-class
     *  REF: https://github.com/SignalR/SignalR/issues/1149#issuecomment-302611992
     */
    public static async Task<T> PromptCallerAsync<T>(
        this Hub hub,
        ConcurrentDictionary<string, TaskCompletionSource<T>> lookup,
        string key,
        string method,
        params object[] args)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        _ = lookup.TryAdd(key, tcs);

        // Using SendCoreAsync() instead of SendAsync() to avoid splitting array of params.
        // I'm justified in using SendCoreAsync() because it's the only way to pass an array of params.
        // REF: https://github.com/aspnet/SignalR/issues/1268
        // REF: https://stackoverflow.com/questions/57979000/microsoft-aspnetcore-signalr-sendasync-args
        // REF: https://stackoverflow.com/questions/51590347/difference-between-sendasync-and-sendcoreasync-methods-in-signalr-core/51590442
        // REF: https://github.com/aspnet/SignalR/issues/2239#issuecomment-387854470
        // REF: https://github.com/dotnet/aspnetcore/blob/a450cb69b5e4549f5515cdb057a68771f56cefd7/src/SignalR/server/Core/src/ClientProxyExtensions.cs
        await (args.Length switch 
        {
            0 => hub.Clients.Caller.SendAsync(method),
            _ => hub.Clients.Caller.SendCoreAsync(method, args)
        });

        // The alternative is to split the params array and use SendAsync() instead of SendCoreAsync().
        // A maximum of 10 parameters can be passed to SendAsync().
        // await (args.Length switch
        // {
        //     0 => hub.Clients.Caller.SendAsync(method),
        //     1 => hub.Clients.Caller.SendAsync(method, args[0]),
        //     2 => hub.Clients.Caller.SendAsync(method, args[0], args[1]),
        //     3 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2]),
        //     4 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3]),
        //     5 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3], args[4]),
        //     6 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3], args[4], args[5]),
        //     7 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6]),
        //     8 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]),
        //     9 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]),
        //     10 => hub.Clients.Caller.SendAsync(method, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]),
        //     _ => throw new NotSupportedException($"{args.Length} parameters are not supported.")
        // });

        try
        {
            // TODO: Cancel this task if the client disconnects (potentially by just adding a timeout)
            return await tcs.Task;
        }
        finally
        {
            _ = lookup.TryRemove(key, out _);
        }
    }

    public static async Task ResolvePromptAsync<T>(
        this Hub hub,
        ConcurrentDictionary<string, TaskCompletionSource<T>> lookup,
        string key,
        T value
    ) {
        if (lookup.TryGetValue(key, out var tcs))
        {
            // Trigger the task continuation
            _ = tcs.TrySetResult(value);
        }
        else
        {
            // Client response for something that isn't being tracked, might be an error
            var message = new SystemMessage
            {
                Text = "You did something unexpected.",
                Type = MessageType.Error
            };
            await hub.Clients.Caller.SendAsync("ReceiveSystemMessage", message);
        }
    }
}