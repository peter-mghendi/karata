using System.Text;
using Karata.Server.Data;
using Karata.Server.Engine;
using Karata.Server.Engine.Exceptions;
using Karata.Server.Hubs.Clients;
using Karata.Server.Services;
using Karata.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using static Karata.Cards.Card.CardFace;

namespace Karata.Server.Hubs;

[Authorize]
public class GameHub : Hub<IGameClient>
{
    private readonly ILogger<GameHub> _logger;
    private readonly IPasswordService _passwordService;
    private readonly KarataContext _context;
    private readonly PresenceService _presence;
    private readonly UserManager<User> _userManager;

    
    // TODO: Move room joining/leaving/creation to the API to enable 
    // clients that don't support SignalR e.g. Telegram
    public GameHub(
        ILogger<GameHub> logger,
        IPasswordService passwordService,
        KarataContext context,
        PresenceService presence,
        UserManager<User> userManager)
    {
        _logger = logger;
        _passwordService = passwordService;
        _context = context;
        _presence = presence;
        _userManager = userManager;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        // User is not logged in - this should not happen.
        if (Context.UserIdentifier is null) return;

        var user = await _userManager.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        _logger.LogInformation("User {User} disconnected.", Context.UserIdentifier);
        
        if (!_presence.TryGetPresence(user.Id, out var rooms) || rooms is null) return;
        foreach (var roomId in rooms)
        {
            _logger.LogInformation("Ending game in room {Room}.", roomId);
            await Clients.Group(roomId).EndGame();
            var room = await _context.Rooms.FindAsync(Guid.Parse(roomId));
            if (room is null) continue;
            room.Game.EndReason = $"{Context.UserIdentifier} disconnected. This game cannot proceed.";
            await _context.SaveChangesAsync();
        }
    }

    public async Task SendChat(string roomId, string text)
    {
        if (Context.UserIdentifier is null) return;
        
        var user = await _userManager.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        var room = await _context.Rooms.FindAsync(Guid.Parse(roomId));
        if (room is null) return;

        var chat = new Chat {Text = text, Sender = user};
        room.Chats.Add(chat);

        await Clients.Group(roomId).ReceiveChat(chat.ToUI());
        await _context.SaveChangesAsync();
    }

    public async Task JoinRoom(string inviteLink, string? password)
    {
        _logger.LogInformation("User {User} is joining room {Room}.", Context.UserIdentifier, inviteLink);

        if (Context.UserIdentifier is null) return;

        var user = await _userManager.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        if (!Guid.TryParse(inviteLink, out var guid)) return;

        var room = await _context.Rooms.FindAsync(guid);
        if (room is null) return;

        var game = room.Game;
        var hand = new Hand {User = user};

        if (room.Hash is not null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                var message = new SystemMessage
                {
                    Text = "You need to enter a password to join this room.",
                    Type = MessageType.Error
                };
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(password);
            if (!_passwordService.VerifyPassword(bytes, room.Salt!, room.Hash))
            {
                var message = new SystemMessage
                {
                    Text = "The password you entered is incorrect.",
                    Type = MessageType.Error
                };
                await Clients.Caller.ReceiveSystemMessage(message);
                return;
            }
        }

        // Check game status
        if (game.IsStarted)
        {
            var message = new SystemMessage
            {
                Text = "This game has already started.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            return;
        }

        // Check player count
        if (game.Hands.Count >= 4)
        {
            var message = new SystemMessage
            {
                Text = "This game is full.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            return;
        }

        // Add player to room
        if (game.Hands.All(h => h.User.Id != hand.User.Id)) game.Hands.Add(hand);
        _presence.AddPresence(user.Id, inviteLink);
        await Clients.OthersInGroup(inviteLink).AddHandToRoom(hand.ToUI());
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
        await Clients.Caller.AddToRoom(room.ToUI());
        await _context.SaveChangesAsync();
    }

    public async Task LeaveRoom(string inviteLink, bool isEnding)
    {
        _logger.LogInformation("User {User} is leaving room {Room}.", Context.UserIdentifier, inviteLink);
     
        if (Context.UserIdentifier is null) return;

        var user = await _userManager.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        var room = await _context.Rooms.FindAsync(Guid.Parse(inviteLink));
        if (room is null) return;

        var game = room.Game;
        var hand = game.Hands.Single(h => h.User.Id == user.Id);

        // Check game status
        if (!isEnding && (game.IsStarted || room.Creator.Id == user.Id))
        {
            // TODO: Handle this gracefully, as well as accidental disconnection.
            var message = new SystemMessage
            {
                Text = "You cannot leave this room while the game is in progress.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            return;
        }

        // Remove player from room
        game.Hands.Remove(hand);
        _presence.RemovePresence(user.Id, inviteLink);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id.ToString());
        await Clients.Caller.RemoveFromRoom();
        await Clients.OthersInGroup(room.Id.ToString()).RemoveHandFromRoom(hand.ToUI());
        await _context.SaveChangesAsync();
    }

    public async Task StartGame(string inviteLink)
    {
        var room = await _context.Rooms.FindAsync(Guid.Parse(inviteLink));
        if (room is null) return;

        var game = room.Game;

        // Check caller role
        if (room.Creator.Id != Context.UserIdentifier)
        {
            var message = new SystemMessage
            {
                Text = "You do not have permission to start this game.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            return;
        }

        // Check game status
        if (game.IsStarted)
        {
            var message = new SystemMessage
            {
                Text = "This game has already started.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            return;
        }

        // Check player number
        if (game.Hands.Count is < 2 or > 4)
        {
            var message = new SystemMessage
            {
                Text = "This game requires 2-4 players.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            return;
        }

        // Shuffle deck and deal starting card
        var deck = game.Deck;
        
        do deck.Shuffle();
        while (!IsBoring(deck.Peek()));
        
        var topCard = deck.Deal();
        _logger.LogInformation("Top card is {Card}.", topCard);

        await Clients.Group(inviteLink).RemoveCardsFromDeck(1);
        game.Pile.Push(topCard);
        await Clients.Group(inviteLink).AddCardRangeToPile(new List<Card> {topCard});
        
        _logger.LogInformation("Dealing cards to players.");

        // Deal player cards
        // TODO: Explicit card movements (Deck -> Hand, Hand -> Pile, etc).
        // Needs more thought: will have to restructure PerformTurn
        foreach (var hand in game.Hands)
        {
            const int dealtCount = 4;

            var dealt = deck.DealMany(dealtCount);
            await Clients.Group(inviteLink).RemoveCardsFromDeck(dealtCount);
            
            _logger.LogInformation("Dealt {Count} cards to {User}.", dealtCount, hand.User.UserName);
            _logger.LogInformation("Cards: {Cards}.", string.Join(", ", dealt.Select(d => d.ToString())));

            hand.Cards.AddRange(dealt);
            await Clients.User(hand.User.Id).AddCardRangeToHand(dealt);
            await Clients.Users(GetOthers(game.Hands, hand)).AddCardsToPlayerHand(hand.ToUI(), dealtCount);
        }
        
        _logger.LogInformation("Done dealing cards.");

        // Start game
        game.IsStarted = true;
        await Clients.Group(inviteLink).UpdateGameStatus(true);
        await _context.SaveChangesAsync();
    }

    // TODO: Refactor this into a service (and add tests) so that it can be shared
    // with the API controller, gRPC service, and/or whatever else.
    public async Task PerformTurn(string roomId, List<Card> cardList)
    {
        // Setup
        var room = await _context.Rooms.FindAsync(Guid.Parse(roomId));
        if (room is null) return;

        var game = room.Game;
        var currentTurn = game.CurrentTurn;
        var player = await _userManager.FindByIdAsync(game.Hands[currentTurn].User.Id);
        if (player is null) return;
        
        var hand = player.Hands.Single(h => h.GameId == game.Id);
        var turn = new Turn {UserId = player.Id, Cards = cardList};
        game.Turns.Add(turn);

        // Check game status
        if (!game.IsStarted)
        {
            var message = new SystemMessage
            {
                Text = "This game has not yet started.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            await Clients.Caller.NotifyTurnProcessed();
            return;
        }

        // Check turn
        if (player.Id != Context.UserIdentifier)
        {
            var message = new SystemMessage
            {
                Text = "It is not your turn.",
                Type = MessageType.Error
            };
            await Clients.Caller.ReceiveSystemMessage(message);
            await Clients.Caller.NotifyTurnProcessed();
            return;
        }

        // game.Give from the last turn becomes Game.Pick for this turn, and game.Give is reset. 
        (game.Pick, game.Give) = (game.Give, 0);

        // Process turn
        try
        {
            KarataEngine.EnsureTurnIsValid(game: game, turnCards: cardList);
        }
        catch (TurnValidationException exception)
        {
            var message = new SystemMessage { Text = exception.Message, Type = MessageType.Error };
            await Clients.Caller.ReceiveSystemMessage(message);
            await Clients.Caller.NotifyTurnProcessed();
            return;
        }

        // Add cards to pile
        foreach (var card in cardList) game.Pile.Push(card);
        await Clients.Group(roomId).AddCardRangeToPile(cardList);

        // Remove cards from player hand
        await Clients.Caller.NotifyTurnProcessed();
        await Clients.Caller.RemoveCardRangeFromHand(cardList);
        await Clients.Users(GetOthers(game.Hands, hand))
            .RemoveCardsFromPlayerHand(hand.ToUI(), cardList.Count);
        hand.Cards.RemoveAll(cardList.Contains);

        // Generate delta and update game state.
        var delta = KarataEngine.GenerateTurnDelta(game, cardList);
        if (delta.RemoveRequestLevels > 0)
        {
            game.CurrentRequest = null;
            await Clients.Group(roomId).SetCurrentRequest(null);
        }

        if (delta.RequestLevel is not GameRequestLevel.NoRequest)
        {
            turn.Request = await Clients.Caller.PromptCardRequest(delta.RequestLevel is GameRequestLevel.CardRequest);
            game.CurrentRequest = turn.Request;
            await Clients.Group(roomId).SetCurrentRequest(turn.Request);
        }

        if (delta.Reverse) game.IsForward = !game.IsForward;
        (game.Give, game.Pick) = (delta.Give, delta.Pick);

        // Check whether there are cards to pick.
        if (game.Pick > 0)
        {
            // Remove card from pile
            if (!game.Deck.TryDealMany(game.Pick, out var cards))
            {
                if (game.Pile.Count + game.Deck.Count - 1 > game.Pick)
                {
                    // Reclaim pile
                    var pileCards = game.Pile.Reclaim();
                    foreach (var pileCard in pileCards) game.Deck.Push(pileCard);
                    await Clients.Group(roomId).ReclaimPile();

                    // Shuffle & deal
                    game.Deck.Shuffle();
                    cards = game.Deck.DealMany(game.Pick);
                }
                else
                {
                    _logger.LogInformation("There are not enough cards to pick. Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}", game.Pile.Count, game.Deck.Count, game.Pick);
                    await Clients.Caller.NotifyTurnProcessed();
                    var message = new SystemMessage
                    {
                        Text = "There aren't enough cards left to pick.",
                        Type = MessageType.Error
                    };
                    await Clients.Group(roomId).ReceiveSystemMessage(message);
                    
                    game.EndReason = $"There aren't enough cards left to pick.";
                    await Clients.Group(roomId).EndGame();
                    await _context.SaveChangesAsync();
                    return;
                }
            }

            await Clients.Group(roomId).RemoveCardsFromDeck(1);

            // Add cards to player hand and reset counter
            hand.Cards.AddRange(cards);
            await Clients.Caller.AddCardRangeToHand(cards);
            await Clients.Users(GetOthers(game.Hands, hand)).AddCardsToPlayerHand(hand.ToUI(), cards.Count);
            game.Pick = 0;
        }

        // Check whether the game is over.
        if (hand.Cards.Count == 0)
        {
            if (hand.IsLastCard && IsBoring(cardList[^1]))
            {
                await Clients.Caller.NotifyTurnProcessed();
                game.EndReason = $"{player.UserName} won";
                game.Winner = player;
                await Clients.Group(roomId).EndGame();
                await _context.SaveChangesAsync();
                return;
            }
            else
            {
                var message = new SystemMessage
                {
                    Text = $"{player.Email} is cardless.",
                    Type = MessageType.Info
                };
                await Clients.OthersInGroup(roomId).ReceiveSystemMessage(message);
            }
        }
        else
        {
            turn.IsLastCard = await Clients.Caller.PromptLastCardRequest();
            if (turn.IsLastCard)
            {
                hand.IsLastCard = true;
                var message = new SystemMessage
                {
                    Text = $"{player.Email} is on their last card.",
                    Type = MessageType.Warning
                };
                await Clients.OthersInGroup(roomId).ReceiveSystemMessage(message);
            }
        }

        // Next turn
        var lastIndex = game.Hands.Count - 1;
        for (uint i = 0; i < delta.Skip; i++)
        {
            if (game.IsForward)
                game.CurrentTurn = currentTurn == lastIndex ? 0 : ++currentTurn;
            else
                game.CurrentTurn = currentTurn == 0 ? lastIndex : --currentTurn;
        }

        await Clients.Group(roomId).UpdateTurn(game.CurrentTurn);
        await _context.SaveChangesAsync();
    }

    private static IEnumerable<string> GetOthers(IEnumerable<Hand> hands, Hand hand) =>
        hands.Where(h => h.User.Id != hand.User.Id).Select(h => h.User.Id);

    private static bool IsBoring(Card card) =>
        !card.IsBomb() && !card.IsQuestion() && card is not {Face: Ace or Jack or King};
}