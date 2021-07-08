using System.Net.Mime;
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
        private readonly IGameService _gameService;
        private readonly IRoomService _roomService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GameHub(
            IEngine engine,
            IGameService gameService,
            IRoomService roomService,
            UserManager<ApplicationUser> userManager) =>
            (_engine, _gameService, _roomService, _userManager) = (engine, gameService, roomService, userManager);

        public async Task SendChatMessage(string inviteLink, string text)
        {
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var message = new ChatMessage { Text = text, Sender = user };

            room.ChatMessages.Add(message);

            await _roomService.UpdateAsync(room);
            await Clients.Group(inviteLink).ReceiveChatMessage(message);
        }

        public async Task CreateRoom()
        {
            // Create room
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            var room = new Room
            {
                InviteLink = Guid.NewGuid().ToString(),
                Creator = user,
            };
            await _roomService.CreateAsync(room);

            // Add player to game
            room.Game.Players.Add(user);
            await _gameService.UpdateAsync(room.Game);
            await Clients.OthersInGroup(room.InviteLink).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.InviteLink);
            await Clients.Caller.AddToRoom(room);
        }

        public async Task JoinRoom(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);

            // Check game status
            if (room.Game.Started)
            {
                var message = new SystemMessage { Text = "This game has already started." };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            }

            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);

            // Add player to room
            room.Game.Players.Add(user);
            await _gameService.UpdateAsync(room.Game);
            await Clients.OthersInGroup(inviteLink).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.InviteLink);
            await Clients.Caller.AddToRoom(room);
        }

        public async Task LeaveRoom(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);

            // Check game status
            if (room.Game.Started || room.Creator.Id == user.Id)
            {
                // TODO: Handle this gracefully, as well as accidental disconnection.
                var message = new SystemMessage
                {
                    Text = "Please don't do this. The game isn't built to handle it."
                };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            }

            // Remove player from room
            room.Game.Players.Remove(user);
            await _gameService.UpdateAsync(room.Game);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.InviteLink);
            await Clients.Caller.RemoveFromRoom();
            await Clients.OthersInGroup(room.InviteLink).RemovePlayerFromRoom(user);
        }

        public async Task StartGame(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var game = room.Game;

            // Check caller role
            if (room.Creator.Email != Context.UserIdentifier)
            {
                var message = new SystemMessage { Text = "You are not allowed to perform that action." };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            };

            // Check player number
            if (game.Players.Count < 2 || game.Players.Count > 4)
            {
                var message = new SystemMessage { Text = "A game needs 2-4 players." };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            };

            // Shuffle deck
            game.Deck.Shuffle();

            // Deal starting card
            int dealtCount = 1;
            var topCard = game.Deck.Deal();
            game.Pile.Cards.Push(topCard);
            await Clients.Group(inviteLink).AddCardToPile(topCard);

            // Deal player cards
            foreach (var player in game.Players)
            {
                dealtCount += 4;
                var dealt = game.Deck.DealMany(4);
                player.Hand.AddRange(dealt);
                await Clients.User(player.Email).AddCardRangeToHand(dealt);
            }

            // TODO: Explicit card movements (Deck -> Hand, Hand -> Pile, etc).
            await Clients.Group(inviteLink).RemoveCardsFromDeck(dealtCount);

            // Start game
            game.Started = true;
            await Clients.Group(inviteLink).UpdateGameStatus(true);
            await _gameService.UpdateAsync(game);
        }

        public async Task<bool> PerformTurn(string inviteLink, List<Card> turn)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var game = room.Game;

            // Check game status
            if (!game.Started)
            {
                var message = new SystemMessage { Text = "The game has not started yet." };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return false;
            }

            // Check turn
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            if (requiredUser.Id != currentUser.Id)
            {
                var message = new SystemMessage{ Text = "It is not your turn!" };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return false;
            }

            // Process turn
            if (!_engine.ValidateTurn(topCard: game.Pile.Cards.Peek(), turnCards: turn))
            {
                var message = new SystemMessage{ Text = "That card sequence is invalid" };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return false;
            }

            // Add cards to deck
            foreach (var card in turn)
            {
                game.Pile.Cards.Push(card);
                await Clients.Group(inviteLink).AddCardToPile(card);
            }

            // Remove cards from player hand
            game.Players.Single(p => p.Email == currentUser.Email).Hand
                .RemoveAll(card => turn.Contains(card));

            // TODO: Post turn actions

            // Update current turn
            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            await Clients.Group(inviteLink).UpdateTurn(game.CurrentTurn);
            await _gameService.UpdateAsync(game);

            return true;
        }

        public async Task PickCard(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var game = room.Game;

            // Check turn
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            if (requiredUser.Id != currentUser.Id)
            {
                var message = new SystemMessage { Text = "It is not your turn!" };
                room.SystemMessages.Add(message);
                await _roomService.UpdateAsync(room);
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            }

            // Remove card from pile
            if (!game.Deck.TryDeal(out Card card))
            {
                if (game.Pile.Cards.Count > 1)
                {
                    // Remove cards from pile
                    var pileCards = game.Pile.Reclaim();
                    await Clients.Group(inviteLink).ReclaimPile();

                    // Add cards to deck
                    foreach (var pileCard in pileCards)
                        game.Deck.Cards.Push(pileCard);
                    await Clients.Group(inviteLink).AddCardsToDeck(pileCards.Count);

                    // Shuffle & deal
                    game.Deck.Shuffle();
                    card = game.Deck.Deal();
                }
                else
                {
                    // TODO: Game over.
                }
            };
            await Clients.Group(inviteLink).RemoveCardsFromDeck(1);

            // Add card to player hand
            game.Players.Single(p => p.Email == Context.UserIdentifier).Hand.Add(card);
            await Clients.Caller.AddCardToHand(card);

            // Update player turn
            var isLastPlayer = game.CurrentTurn == game.Players.Count - 1;
            game.CurrentTurn = isLastPlayer ? 0 : game.CurrentTurn + 1;
            await Clients.Group(inviteLink).UpdateTurn(game.CurrentTurn);
            await _gameService.UpdateAsync(game);
        }
    }
}