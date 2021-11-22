#nullable enable

using Karata.Web.Hubs.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Web.Hubs;

[Authorize]
public class RequestHub : Hub<IRequestClient>
{
    public void RequestCard(Guid identifier, Card request)
    {
        if (GameHub.CardRequests.TryGetValue(identifier, out var tcs))
        {
            // Trigger the task continuation
            tcs.TrySetResult(request);
        }
        else
        {
            // Client response for something that isn't being tracked, might be an error
        }
    }

    public void SetLastCardStatus(Guid identifier, bool lastCard)
    {
        if (GameHub.LastCardRequests.TryGetValue(identifier, out var tcs))
        {
            // Trigger the task continuation
            tcs.TrySetResult(lastCard);
        }
        else
        {
            // Client response for something that isn't being tracked, might be an error
        }
    }
}