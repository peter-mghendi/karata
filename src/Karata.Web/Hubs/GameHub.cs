using System;
using System.Collections.Concurrent;
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

namespace Karata.Web.Hubs
{
    /** 
     *  NOTE:
     *  Here be dragons.
     *  Implemented herein is (one half of) a mechanism to prompt for values from the frontend.
     *  (See adjacent RequestHub for the other half.)
     *  I use a ConcurrentDictionary, TaskCompletionSource<T> and EAP for this.
     *
     *  REF: https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-wrap=eap-patterns-in-a-task
     *  REF: https://devblogs.microsoft.com/premier-developer/the-danger-of-the-taskcompletionsourcet-class
     *  REF: https://github.com/SignalR/SignalR/issues/1149#issuecomment-302611992
     */
    [Authorize]
    public class GameHub : Hub<IGameClient>
    {
        private readonly IEngine _engine;
        // private readonly ILogger<GameHub> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoomService _roomService;
        private readonly UserManager<ApplicationUser> _userManager;
        public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<Card>> CardRequests = new();
        // public static readonly ConcurrentDictionary<Guid, TaskCompletionSource<bool>> LastCardRequests = new();

        public GameHub(
            IEngine engine,
            // ILogger<GameHub> logger,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _engine = engine;
            // _logger = logger;
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

            // Check game status
            if (game.IsStarted)
            {
                await Clients.Caller.ReceiveSystemMessage("This game has already begun.");
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

        public async Task PerformTurn(string inviteLink, List<Card> cardList)
        {
            var room = await _roomService.FindByInviteLinkAsync(inviteLink);

            // Check game status
            if (!room.Game.IsStarted)
            {
                await Clients.Caller.ReceiveSystemMessage("The game has not started yet.");
                await Clients.Caller.NotifyTurnProcessed(valid: false);
                return;
            }

            // Check turn
            var requiredUser = room.Game.Players[room.Game.CurrentTurn];
            var currentUser = await _userManager.FindByEmailAsync(Context.UserIdentifier);
            if (requiredUser.Id != currentUser.Id)
            {
                await Clients.Caller.ReceiveSystemMessage("It is not your turn!");
                await Clients.Caller.NotifyTurnProcessed(valid: false);
                return;
            }

            // Cards from last turn
            room.Game.Pick = room.Game.Give;
            room.Game.Give = 0;

            // Process turn
            if (!_engine.ValidateTurnCards(room.Game, cardList))
            {
                await Clients.Caller.ReceiveSystemMessage("That card sequence is invalid");
                await Clients.Caller.NotifyTurnProcessed(valid: false);
                return;
            }

            // Add cards to pile
            foreach (var card in cardList)
            {
                room.Game.Pile.Push(card);
                await Clients.Group(inviteLink).AddCardToPile(card);
            }

            // Remove cards from player hand
            room.Game.Players.Single(p => p.Email == currentUser.Email).Hand
                .RemoveAll(card => cardList.Contains(card));

            // Generate delta and update game state.
            var delta = _engine.GenerateTurnDelta(room.Game, cardList);
            if (delta.HasRequest)
            {
                // TODO Handle GUID collisions.
                // GUID identifier and TaskCompletionSource for parallel requests.
                var identifier = Guid.NewGuid();
                var tcs = new TaskCompletionSource<Card>();
                CardRequests.TryAdd(identifier, tcs);

                // TODO: Separate card and suit requests.
                await Clients.Caller.PromptCardRequest(identifier, delta.HasSpecificRequest);

                try
                {
                    // Wait for the client to respond
                    // TODO: Cancel this task if the client disconnects (potentially by just adding a timeout)
                    var card = await tcs.Task;
                    Console.WriteLine($"Request: {card}, Specific: {delta.HasSpecificRequest}");
                    // room.Game.CurrentRequest = await tcs.Task;
                }
                finally
                {
                    // Remove the tcs from the dictionary so that we don't leak memory
                    CardRequests.TryRemove(identifier, out tcs);
                }
            }
            if (delta.Reverse)
            {
                room.Game.IsForward = !room.Game.IsForward;
            }
            room.Game.Give = delta.Give;
            room.Game.Pick = delta.Pick;

            // Check whether there are cards to pick.
            if (room.Game.Pick > 0)
            {
                // Remove card from pile
                if (!room.Game.Deck.TryDealMany(room.Game.Pick, out List<Card> cards))
                {
                    if (room.Game.Pile.Count + room.Game.Deck.Count - 1 > room.Game.Pick)
                    {
                        // Remove cards from pile
                        var pileCards = room.Game.Pile.Reclaim();
                        await Clients.Group(inviteLink).ReclaimPile();

                        // Add cards to deck
                        foreach (var pileCard in pileCards)
                            room.Game.Deck.Push(pileCard);
                        await Clients.Group(inviteLink)
                            .AddCardsToDeck((uint)pileCards.Count);

                        // Shuffle & deal
                        room.Game.Deck.Shuffle();
                        cards = room.Game.Deck.DealMany(room.Game.Pick);
                    }
                    else
                    {
                        // TODO: Game over.
                    }
                };
                await Clients.Group(inviteLink).RemoveCardsFromDeck(1);

                // Add cards to player hand
                room.Game.Players.Single(p => p.Email == Context.UserIdentifier).Hand.AddRange(cards);
                await Clients.Caller.AddCardRangeToHand(cards);

                // Reset pick counter
                room.Game.Pick = 0;
            }

            // TODO: Check whether the game is over.
            // var player = room.Game.Players.Single(p => p.Email == currentUser.Email);
            // if (player.Hand.Count == 0 && player.IsLastCard) GameOver();

            // var lastCardIdentifier = Guid.NewGuid();
            // var lastCardTcs = new TaskCompletionSource<bool>();
            // LastCardRequests.TryAdd(lastCardIdentifier, lastCardTcs);
            // await Clients.Caller.PromptLastCardRequest(lastCardIdentifier);

            // try
            // {
            //     // Wait for the client to respond
            //     // TODO: Cancel this task if the client disconnects (potentially by just adding a timeout)
            //     var isLastCard = await lastCardTcs.Task;
            //     Console.WriteLine($"Last card: {isLastCard}");
            // }
            // finally
            // {
            //     // Remove the tcs from the dictionary so that we don't leak memory
            //     LastCardRequests.TryRemove(lastCardIdentifier, out lastCardTcs);
            // }

            // Next turn
            var lastIndex = room.Game.Players.Count - 1;
            for (uint i = 0; i < delta.Skip; i++)
            {
                if (room.Game.IsForward)
                {
                    room.Game.CurrentTurn = room.Game.CurrentTurn == lastIndex
                        ? 0 : room.Game.CurrentTurn + 1;
                }
                else
                {
                    room.Game.CurrentTurn = room.Game.CurrentTurn == 0
                        ? lastIndex : room.Game.CurrentTurn - 1;
                }
            }

            await Clients.Group(inviteLink).UpdateTurn(room.Game.CurrentTurn);
            await _unitOfWork.CompleteAsync();

            await Clients.Caller.NotifyTurnProcessed(valid: true);
        }
    }
}