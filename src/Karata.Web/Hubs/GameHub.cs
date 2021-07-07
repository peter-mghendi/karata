using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Karata.Cards;
using Karata.Web.Engines;
using Karata.Web.Hubs.Clients;
using Karata.Web.Models;
using Karata.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Karata.Web.Hubs
{
    // TODO Run async calls concurrently
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        private readonly IEngine _engine;
        private readonly IRoomService _roomService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GameHub(
            IEngine engine,
            IRoomService roomService,
            UserManager<ApplicationUser> userManager) =>
            (_engine, _roomService, _userManager)  = (engine, roomService, userManager);

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
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);
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

            _roomService.Update(room);
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

            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);

            // Add player to room
            room.Game.Players.Add(user);
            await Clients.OthersInGroup(roomLink).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.AddToRoom(room);

            _roomService.Update(room);
        }

        public async Task LeaveRoom(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);

            // Check game status
            if (room.Game.Started || room.Creator.Id == user.Id)
            {
                // TODO: Handle this gracefully, as well as accidental disconnection.
                await Clients.Caller.ReceiveSystemMessage(new("Please don't do this. The game isn't built to handle it."));
                return;
            }

            // Remove player from room
            room.Game.Players.Remove(user);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Link);
            await Clients.Caller.RemoveFromRoom();
            await Clients.OthersInGroup(room.Link).RemovePlayerFromRoom(user);

            _roomService.Update(room);
        }

        public async Task StartGame(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];
            var game = room.Game;

            // Check caller role
            if (room.Creator.Email != Context.UserIdentifier)
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
                player.Hand.AddRange(dealt);
                await Clients.User(player.Email).AddCardRangeToHand(dealt);
            }

            // TODO: Explicit card movements (Deck -> Hand, Hand -> Pile, etc).
            await Clients.Group(roomLink).RemoveCardsFromDeck(dealtCount);

            // Start game
            game.Started = true;
            await Clients.Group(roomLink).UpdateGameStatus(true);

            room.Game = game;
            _roomService.Update(room);
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
            var currentUser = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            if (requiredUser.Id != currentUser.Id)
            {
                await Clients.Caller.ReceiveSystemMessage(new("It is not your turn!"));
                return false;
            }

            // Process turn
            if (!_engine.ValidateTurn(topCard: game.Pile.Cards.Peek(), turnCards: turn))
            {
                await Clients.Caller.ReceiveSystemMessage(new("That card sequence is invalid"));
                return false;
            }

            // Add cards to deck
            foreach (var card in turn)
            {
                game.Pile.Cards.Push(card);
                await Clients.Group(roomLink).AddCardToPile(card);
            }

            // Remove cards from player hand
            game.Players.Single(p => p.Email == currentUser.Email).Hand
                .RemoveAll(card => turn.Contains(card));

            // Update current turn
            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            await Clients.Group(roomLink).UpdateTurn(game.CurrentTurn);

            room.Game = game;
            _roomService.Update(room);
            return true;
        }

        public async Task PickCard(string roomLink)
        {
            var room = _roomService.Rooms[roomLink];
            var game = room.Game;

            // Check turn
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            if (requiredUser.Id != currentUser.Id)
            {
                await Clients.Caller.ReceiveSystemMessage(new("It is not your turn!"));
                return;
            }

            // Remove card from pile
            if (!game.Deck.TryDeal(out Card card))
            {
                if (game.Pile.Cards.Count > 1)
                {
                    // Remove cards from pile
                    var pileCards = game.Pile.Reclaim();
                    await Clients.Group(roomLink).ReclaimPile();

                    // Add cards to deck
                    foreach (var pileCard in pileCards)
                        game.Deck.Cards.Push(pileCard);
                    await Clients.Group(roomLink).AddCardsToDeck(pileCards.Count);

                    // Shuffle & deal
                    game.Deck.Shuffle();
                    card = game.Deck.Deal();
                }
                else
                {
                    // TODO: Game over.
                }
            };
            await Clients.Group(roomLink).RemoveCardsFromDeck(1);

            // Add card to player hand
            game.Players.Single(p => p.Email == Context.UserIdentifier).Hand.Add(card);
            await Clients.Caller.AddCardToHand(card);

            // Update player turn
            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            await Clients.Group(roomLink).UpdateTurn(game.CurrentTurn);

            room.Game = game;
            _roomService.Update(room);
        }
    }
}