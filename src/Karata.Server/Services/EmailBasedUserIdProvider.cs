using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public class EmailBasedUserIdProvider : IUserIdProvider
{
    public virtual string? GetUserId(HubConnectionContext connection) => 
        connection.User.FindFirst(ClaimTypes.Email)?.Value;
}