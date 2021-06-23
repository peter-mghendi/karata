using System;
using System.Threading.Tasks;
using Karata.Web.Hubs.Clients;
using Karata.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Karata.Web.Services;
using Microsoft.Extensions.Logging;

namespace Karata.Web.Hubs
{
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        private readonly ILogger<GameHub> _logger;
        private readonly IRoomService _roomService;

        public GameHub(IRoomService roomService, ILogger<GameHub> logger) => 
            (_roomService, _logger) = (roomService, logger);

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
            var user = new User(Context.UserIdentifier);
            var room = new Room
            {
                Link = Guid.NewGuid().ToString(),
                Creator = user,
            };
            _roomService.Rooms.Add(room.Link, room);
            await AddToRoomAsync(room: room, user: user);
        }

        public async Task JoinRoom(string roomLink) =>
            await AddToRoomAsync(room: _roomService.Rooms[roomLink], user: new(Context.UserIdentifier));

        public async Task LeaveRoom(string roomLink)
        {
            _roomService.Rooms[roomLink].Game.Players.Remove(new User(Context.UserIdentifier));

            var room = _roomService.Rooms[roomLink];
            var message = new SystemMessage()
            {
                Text = $"{Context.User.Identity.Name} left the room.",
                Sent = DateTime.Now
            };

            await Groups.RemoveFromGroupAsync(connectionId: Context.ConnectionId, groupName: room.Link);
            await Clients.Caller.RemoveFromRoom();
            await Clients.Group(groupName: room.Link).UpdateGameInfo(game: room.Game);
            await Clients.Group(groupName: room.Link).ReceiveSystemMessage(message: message);
        }

        public async Task PerformTurn(string roomLink)
        {
            var game = _roomService.Rooms[roomLink].Game;
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = new User(Context.UserIdentifier);

            if(requiredUser != currentUser)
            {
                await Clients.Caller.ReceiveSystemMessage(new() { Text = "It is not your turn!"});
                return;
            }

            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            _roomService.Rooms[roomLink].Game = game;

            await Clients.Group(roomLink).TurnPerformed(player: currentUser);
            await Clients.Group(roomLink).UpdateGameInfo(game: game);
        }

        private async Task AddToRoomAsync(Room room, User user)
        {
            var message = new SystemMessage()
            {
                Text = $"{Context.User.Identity.Name} joined the room.",
                Sent = DateTime.Now
            };

            _roomService.Rooms[room.Link].Game.Players.Add(user);
            room = _roomService.Rooms[room.Link];

            await Groups.AddToGroupAsync(connectionId: Context.ConnectionId, groupName: room.Link);
            await Clients.Caller.AddToRoom(room: room);
            await Clients.OthersInGroup(groupName: room.Link).UpdateGameInfo(game: room.Game);
            await Clients.Group(groupName: room.Link).ReceiveSystemMessage(message: message);
        }
    }
}