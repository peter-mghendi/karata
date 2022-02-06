using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Web.Extensions;

public static class HubExtensions
{
    /** 
     *  TODO: Timeouts and error handling.
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

        await (args.Length switch 
        {
            0 => hub.Clients.Caller.SendAsync(method),
            _ => hub.Clients.Caller.SendCoreAsync(method, args)
        });

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
            _ = tcs.TrySetResult(value);
        }
        else
        {
            var message = new SystemMessage
            {
                Text = "You did something unexpected.",
                Type = MessageType.Error
            };
            await hub.Clients.Caller.SendAsync("ReceiveSystemMessage", message);
        }
    }
}