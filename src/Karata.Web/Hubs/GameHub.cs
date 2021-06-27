using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karata.Cards;
using Karata.Web.Hubs.Clients;
using Karata.Web.Models;
using Karata.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Web.Hubs
{
    // TODO Run async calls concurrently
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
                Sender = Context.User.Identity.Name
            };
            _roomService.Rooms[roomLink].Messages.Add(message);
            await Clients.Group(roomLink).ReceiveChatMessage(message);
        }

        public async Task CreateRoom()
        {
            // Create room
            var user = new User(Context.UserIdentifier);
            var room = new Room
            {
                Link = Guid.NewGuid().ToString(),
                Creator = user,
            };
            _roomService.Rooms.Add(room.Link, room);

            // Add player to room
            room.Game.Players.Add(user);
            await Clients.OthersInGroup(room.Link).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.AddToRoom(room);

            SaveRoomState(room);
        }

        public async Task JoinRoom(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];

            // Check game status
            if (room.Game.Started)
            {
                await Clients.Caller.ReceiveSystemMessage(new("This game has already started."));
                return;
            }

            // Add player to room
            var user = new User(Context.UserIdentifier);
            room.Game.Players.Add(user);
            await Clients.OthersInGroup(roomLink).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.AddToRoom(room);

            SaveRoomState(room);
        }

        public async Task LeaveRoom(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];

            // Check game status
            if (room.Game.Started)
            {
                await Clients.Caller.ReceiveSystemMessage(new("Please don't do this. The game isn't built to handle it."));
                return;
            }

            var user = new User(Context.UserIdentifier);

            // Remove player from room
            room.Game.Players.Remove(user);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.RemoveFromRoom();
            await Clients.OthersInGroup(room.Link).RemovePlayerFromRoom(user);

            SaveRoomState(room);
        }

        public async Task StartGame(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];
            var game = room.Game;

            // Check caller role
            if (room.Creator.Username != Context.UserIdentifier)
            {
                await Clients.Caller.ReceiveSystemMessage(new("You are not allowed to perform that action."));
                return;
            };

            // Check player number
            if (game.Players.Count < 2 || game.Players.Count > 4)
            {
                await Clients.Caller.ReceiveSystemMessage(new("A game needs 2-4 players."));
                return;
            };

            // Shuffle deck
            game.Deck.Shuffle();

            // Deal starting card
            int dealtCount = 1;
            var topCard = game.Deck.Deal();
            game.Pile.Cards.Push(topCard);
            await Clients.Group(roomLink).AddCardToPile(topCard);

            // Deal player cards
            foreach (var player in game.Players)
            {
                dealtCount += 4;
                var dealt = game.Deck.DealMany(4);
                player.Hand.Cards.AddRange(dealt);
                await Clients.User(player.Username).AddCardRangeToHand(dealt);
            }
            
            await Clients.Group(roomLink).RemoveCardsFromDeck(dealtCount);

            // Start game
            game.Started = true;
            await Clients.Group(roomLink).UpdateGameStatus(true);

            room.Game = game;
            SaveRoomState(room);
        }

        public async Task<bool> PerformTurn(string roomLink, List<Card> turn)
        {
            var room = _roomService.Rooms[roomLink];
            var game = room.Game;

            // Check game status
            if (!game.Started)
            {
                await Clients.Caller.ReceiveSystemMessage(new("The game has not started yet."));
                return false;
            }

            // Check turn
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = new User(Context.UserIdentifier);
            if (requiredUser.Username != currentUser.Username)
            {
                await Clients.Caller.ReceiveSystemMessage(new("It is not your turn!"));
                return false;
            }

            // Add cards to deck
            foreach (var card in turn)
            {
                game.Pile.Cards.Push(card);
                await Clients.Group(roomLink).AddCardToPile(card);
            }

            // Remove cards from player hand
            game.Players.Single(p => p.Username == currentUser.Username).Hand.Cards
                .RemoveAll(card => turn.Contains(card));

            // Update current tuen
            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            await Clients.Group(roomLink).UpdateTurn(game.CurrentTurn);

            room.Game = game;
            SaveRoomState(room);
            return true;
        }

        public async Task PickCard(string roomLink)
        {
            var room =  _roomService.Rooms[roomLink];
            var game = room.Game;

            // Check turn
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = new User(Context.UserIdentifier);
            if (requiredUser.Username != currentUser.Username)
            {
                await Clients.Caller.ReceiveSystemMessage(new("It is not your turn!"));
                return;
            }

            // Remove card from pile
            var card = game.Deck.Deal();
            await Clients.Group(roomLink).RemoveCardsFromDeck(1);

    	    // Add card to player hand
            game.Players.Single(p => p.Username == Context.UserIdentifier).Hand.Cards
                .Add(card);
            await Clients.Caller.AddCardToHand(card);

            // Update player turn
            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            await Clients.Group(roomLink).UpdateTurn(game.CurrentTurn);

            room.Game = game;
            SaveRoomState(room);
        }

        // Save server-side state
        private void SaveRoomState(Room room) => _roomService.Rooms[room.Link] = room;
    }
}