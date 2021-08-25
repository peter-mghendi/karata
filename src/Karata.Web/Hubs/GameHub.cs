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
        private readonly ILogger<GameHub> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoomService _roomService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GameHub(
            IEngine engine,
            ILogger<GameHub> logger,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _engine = engine;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _roomService = _unitOfWork.RoomService;
            _userManager = userManager;
        }

        public async Task SendChat(string inviteLink, string text)
        {
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var message = new Chat { Text = text, Sender = user };

            room.Chats.Add(message);

            await Clients.Group(inviteLink).ReceiveChat(message);
            await _unitOfWork.CompleteAsync();
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
            await Clients.OthersInGroup(room.InviteLink).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.InviteLink);
            await Clients.Caller.AddToRoom(room);
            await _unitOfWork.CompleteAsync();
        }

        public async Task JoinRoom(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);

            // Check game status
            if (room.Game.IsStarted)
            {
                await Clients.Caller.ReceiveSystemMessage("This game has already started.");
                return;
            }

            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);

            // Add player to room
            room.Game.Players.Add(user);
            await Clients.OthersInGroup(inviteLink).AddPlayerToRoom(user);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.InviteLink);
            await Clients.Caller.AddToRoom(room);
            await _unitOfWork.CompleteAsync();
        }

        public async Task LeaveRoom(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var user = await _userManager.FindByEmailAsync(Context.UserIdentifier);

            // Check game status
            if (room.Game.IsStarted || room.Creator.Id == user.Id)
            {
                // TODO: Handle this gracefully, as well as accidental disconnection.
                await Clients.Caller.ReceiveSystemMessage("Please don't do this. The game isn't built to handle it.");
                return;
            }

            // Remove player from room
            room.Game.Players.Remove(user);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.InviteLink);
            await Clients.Caller.RemoveFromRoom();
            await Clients.OthersInGroup(room.InviteLink).RemovePlayerFromRoom(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task StartGame(string inviteLink)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var game = room.Game;

            // Check caller role
            if (room.Creator.Email != Context.UserIdentifier)
            {
                await Clients.Caller.ReceiveSystemMessage("You are not allowed to perform that action.");
                return;
            };

            // Check player number
            if (game.Players.Count < 2 || game.Players.Count > 4)
            {
                await Clients.Caller.ReceiveSystemMessage("A game needs 2-4 players.");
                return;
            };

            // Shuffle deck
            var deck = game.Deck;
            deck.Shuffle();

            // Deal starting card
            var topCard = deck.Deal();
            await Clients.Group(inviteLink).RemoveCardsFromDeck(1);
            game.Pile.Push(topCard);
            await Clients.Group(inviteLink).AddCardToPile(topCard);

            // Deal player cards
            // TODO: Explicit card movements (Deck -> Hand, Hand -> Pile, etc).
            foreach (var player in game.Players)
            {
                player.Hand.Clear();
                await Clients.User(player.Email).EmptyHand();

                uint dealtCount = 4;

                var dealt = deck.DealMany(dealtCount);
                await Clients.Group(inviteLink).RemoveCardsFromDeck(dealtCount);

                player.Hand.AddRange(dealt);
                await Clients.User(player.Email).AddCardRangeToHand(dealt);
            }

            // Start game
            game.IsStarted = true;
            await Clients.Group(inviteLink).UpdateGameStatus(true);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> PerformTurn(string inviteLink, List<Card> cardList)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);
            var game = room.Game;

            // Check game status
            if (!game.IsStarted)
            {
                await Clients.Caller.ReceiveSystemMessage("The game has not started yet.");
                return false;
            }

            // Check turn
            var requiredUser = game.Players[game.CurrentTurn];
            var currentUser = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            if (requiredUser.Id != currentUser.Id)
            {
                await Clients.Caller.ReceiveSystemMessage("It is not your turn!");
                return false;
            }

            // Process turn
            if (!_engine.ValidateTurnCards(game, cardList))
            {
                await Clients.Caller.ReceiveSystemMessage("That card sequence is invalid");
                return false;
            }

            // Add cards to pile
            foreach (var card in cardList)
            {
                game.Pile.Push(card);
                await Clients.Group(inviteLink).AddCardToPile(card);
            }

            // Remove cards from player hand
            game.Players.Single(p => p.Email == currentUser.Email).Hand
                .RemoveAll(card => cardList.Contains(card));

            // Post turn actions
            // EXPERIMENTAL!
            var delta = _engine.GenerateTurnDelta(game, cardList);
            game.ApplyGameDelta(delta);

            // Check whether there are cards to pick.
            if (game.Pick > 0)
            {
                // Remove card from pile
                if (!game.Deck.TryDealMany(game.Pick, out List<Card> cards))
                {
                    if (game.Pile.Count > game.Pick)
                    {
                        // Remove cards from pile
                        var pileCards = game.Pile.Reclaim();
                        await Clients.Group(inviteLink).ReclaimPile();

                        // Add cards to deck
                        foreach (var pileCard in pileCards)
                            game.Deck.Push(pileCard);
                        await Clients.Group(inviteLink)
                            .AddCardsToDeck((uint)pileCards.Count);

                        // Shuffle & deal
                        game.Deck.Shuffle();
                        cards = game.Deck.DealMany(game.Pick);
                    }
                    else
                    {
                        // TODO: Game over.
                    }
                };
                await Clients.Group(inviteLink).RemoveCardsFromDeck(1);

                // Add cards to player hand
                game.Players.Single(p => p.Email == Context.UserIdentifier).Hand.AddRange(cards);
                await Clients.Caller.AddCardRangeToHand(cards);

                // Reset pick counter
                game.Pick = 0;
            }

            await Clients.Group(inviteLink).UpdateTurn(game.CurrentTurn);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}