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

        public async Task SendChatMessage(string roomLink, string text) =>
            await Clients.Group(roomLink).ReceiveChatMessage(new()
            {
                Text = text,
                Sender = Context.User.Identity.Name
            });

        public async Task CreateRoom()
        {
            var user = new User(Context.UserIdentifier);
            var room = new Room
            {
                Link = Guid.NewGuid().ToString(),
                Creator = user,
            };
            _roomService.Rooms.Add(room.Link, room);
            await AddToRoomAsync(room, user);
        }

        public async Task JoinRoom(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];
            if (room.Game.Started)
            {
                await Clients.Caller.ReceiveSystemMessage(new("This game has already started."));
                return;
            }
            await AddToRoomAsync(room, new(Context.UserIdentifier));
        }

        public async Task LeaveRoom(string roomLink)
        {
            var user = new User(Context.UserIdentifier);
            _roomService.Rooms[roomLink].Game.Players.Remove(user);
            var room = _roomService.Rooms[roomLink];

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.RemoveFromRoom();
            await Clients.Group(room.Link).UpdateGameInfo(room.Game);
            await Clients.Group(room.Link).ReceiveSystemMessage(new($"{user.Username} left the room."));
        }

        public async Task StartGame(string roomLink)
        {
            if (_roomService.Rooms[roomLink].Creator.Username != Context.UserIdentifier)
            {
                await Clients.Caller.ReceiveSystemMessage(new("You are not allowed to perform that action."));
                return;
            };

            _roomService.Rooms[roomLink].Game.Started = true;
            var room = _roomService.Rooms[roomLink];

            await Clients.Group(room.Link).UpdateGameInfo(room.Game);
            await Clients.Group(room.Link).ReceiveSystemMessage(new("The game has started. No new players may join."));
        }

        public async Task PerformTurn(string roomLink)
        {
            var game = _roomService.Rooms[roomLink].Game;
            if (!game.Started)
            {
                await Clients.Caller.ReceiveSystemMessage(new("The game has not started yet."));
                return;
            }

            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = new User(Context.UserIdentifier);
            if (requiredUser != currentUser)
            {
                await Clients.Caller.ReceiveSystemMessage(new("It is not your turn!"));
                return;
            }

            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            _roomService.Rooms[roomLink].Game = game;

            await Clients.Group(roomLink).TurnPerformed(currentUser);
            await Clients.Group(roomLink).UpdateGameInfo(game);
        }

        private async Task AddToRoomAsync(Room room, User user)
        {
            _roomService.Rooms[room.Link].Game.Players.Add(user);
            room = _roomService.Rooms[room.Link];

            await Groups.AddToGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.AddToRoom(room);
            await Clients.OthersInGroup(room.Link).UpdateGameInfo(room.Game);
            await Clients.OthersInGroup(room.Link).ReceiveSystemMessage(new($"{user.Username} joined the room."));
        }
    }
}