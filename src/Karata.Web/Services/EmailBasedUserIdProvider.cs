using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Web.Services
{
    public class EmailBasedUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection) => 
            connection.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}