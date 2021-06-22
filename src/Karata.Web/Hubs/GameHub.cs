using System;
using System.Threading.Tasks;
using Karata.Web.Hubs.Clients;
using Karata.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Karata.Web.Services;

namespace Karata.Web.Hubs
{
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        private readonly IRoomService _roomService;

        public GameHub(IRoomService roomService) => _roomService = roomService;

        public async Task SendChatMessage(string roomLink, string text)
        {
            var message = new ChatMessage
            {
                Text = text,
                Sender = Context.User.Identity.Name,
                Sent = DateTime.Now
            };
            await Clients.Group(groupName: roomLink).ReceiveChatMessage(message: message);
        }

        public async Task CreateRoom()
        {
            var user = new User { Username = Context.UserIdentifier };
            var room = new Room
            {
                Link = Guid.NewGuid().ToString(),
                Creator = user,
            };
            _roomService.rooms.Add(room.Link, room);
            await AddToRoomAsync(room: room, user: user);
        }

        public async Task JoinRoom(string roomLink) =>
            await AddToRoomAsync(room: _roomService.rooms[roomLink], user: new()
            {
                Username = Context.UserIdentifier
            });

        public async Task LeaveRoom(string roomLink)
        {
            _roomService.rooms[roomLink].Members.RemoveAll(u => u.Username == Context.UserIdentifier);

            var room = _roomService.rooms[roomLink];
            var message = new SystemMessage()
            {
                Text = $"{Context.User.Identity.Name} left the room.",
                Sent = DateTime.Now
            };

            await Groups.RemoveFromGroupAsync(connectionId: Context.ConnectionId, groupName: room.Link);
            await Clients.Caller.RemoveFromRoom();
            await Clients.Group(groupName: room.Link).UpdateRoomMembers(members: room.Members);
            await Clients.Group(groupName: room.Link).ReceiveSystemMessage(message: message);
        }

        private async Task AddToRoomAsync(Room room, User user)
        {
            var message = new SystemMessage()
            {
                Text = $"{Context.User.Identity.Name} joined the room.",
                Sent = DateTime.Now
            };

            _roomService.rooms[room.Link].Members.Add(user);
            room = _roomService.rooms[room.Link];

            await Groups.AddToGroupAsync(connectionId: Context.ConnectionId, groupName: room.Link);
            await Clients.Caller.AddToRoom(room: room);
            await Clients.OthersInGroup(groupName: room.Link).UpdateRoomMembers(members: room.Members);
            await Clients.Group(groupName: room.Link).ReceiveSystemMessage(message: message);
        }
    }
}