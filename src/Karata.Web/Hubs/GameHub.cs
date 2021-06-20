using System;
using System.Threading.Tasks;
using Karata.Web.Hubs.Clients;
using Karata.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Web.Hubs
{
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        public override async Task OnConnectedAsync()
        {
            // TODO Provide this client with a list of connected users
            var message = new SystemMessage()
            {
                Text = $"{Context.User.Identity.Name} joined the game.",
                Sent = DateTime.Now
            };
            await Clients.All.ReceiveSystemMessage(message);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var message = new SystemMessage()
            {
                Text = $"{Context.User.Identity.Name} left the game.",
                Sent = DateTime.Now
            };
            await Clients.All.ReceiveSystemMessage(message);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendChatMessage(string text)
        {
            var message = new ChatMessage()
            {
                Text = text,
                Sender = Context.User.Identity.Name,
                Sent = DateTime.Now
            };
            await Clients.All.ReceiveChatMessage(message);
        }
    }
}