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
using Microsoft.EntityFrameworkCore;

namespace Karata.Server.Hubs;

[Authorize]
public class GameHub : Hub<IGameClient>
{
    private readonly ILogger<GameHub> _logger;
    private readonly IPasswordService _passwords;
    private readonly KarataContext _context;
    private readonly PresenceService _presence;
    private readonly UserManager<User> _users;


    // TODO: Move room joining/leaving/creation to the API to enable 
    // clients that don't support SignalR e.g. Telegram
    public GameHub(
        ILogger<GameHub> logger,
        IPasswordService passwords,
        KarataContext context,
        PresenceService presence,
        UserManager<User> users)
    {
        _logger = logger;
        _passwords = passwords;
        _context = context;
        _presence = presence;
        _users = users;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        // User is not logged in - this should not happen.
        if (Context.UserIdentifier is null) return;

        var user = await _users.FindByIdAsync(Context.UserIdentifier);
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

        var user = await _users.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        var room = await _context.Rooms.FindAsync(Guid.Parse(roomId));
        if (room is null) return;

        var chat = new Chat { Text = text, Sender = user };
        room.Chats.Add(chat);

        await Clients.Group(roomId).ReceiveChat(chat.ToData());
        await _context.SaveChangesAsync();
    }

    public async Task JoinRoom(string inviteLink, string? password)
    {
        _logger.LogInformation("User {User} is joining room {Room}.", Context.UserIdentifier, inviteLink);

        if (Context.UserIdentifier is null) return;

        var user = await _users.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        if (!Guid.TryParse(inviteLink, out var guid)) return;

        // TODO: Querying
        var room = await _context.Rooms.FindAsync(guid);
        
        if (room is null) return;

        var game = room.Game;
        var hand = new Hand { User = user };

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
            if (!_passwords.VerifyPassword(bytes, room.Salt!, room.Hash))
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
        await Clients.OthersInGroup(inviteLink).AddHandToRoom(hand.ToData());
        await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());
        await Clients.Caller.AddToRoom(room.ToData());
        await _context.SaveChangesAsync();
    }

    public async Task LeaveRoom(string inviteLink, bool isEnding)
    {
        _logger.LogInformation("User {User} is leaving room {Room}.", Context.UserIdentifier, inviteLink);

        if (Context.UserIdentifier is null) return;

        var user = await _users.FindByIdAsync(Context.UserIdentifier);
        if (user is null) return;

        var room = await _context.Rooms
            .Include(r => r.Creator)
            .Include(r => r.Game)
            .ThenInclude(g => g.Hands)
            .ThenInclude(h => h.User)
            .SingleOrDefaultAsync(r => r.Id == Guid.Parse(inviteLink));

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
        await Clients.OthersInGroup(room.Id.ToString()).RemoveHandFromRoom(hand.ToData());
        await _context.SaveChangesAsync();
    }

    public async Task StartGame(string inviteLink)
    {
        var room = await _context.Rooms
            .Include(r => r.Creator)
            .Include(r => r.Game)
            .ThenInclude(g => g.Hands)
            .ThenInclude(h => h.User)
            .SingleOrDefaultAsync(room => room.Id == Guid.Parse(inviteLink));

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
        while (deck.Peek().IsSpecial());

        var top = deck.Deal();
        _logger.LogInformation("Top card is {Card}.", top);

        await Clients.Group(inviteLink).RemoveCardsFromDeck(1);
        game.Pile.Push(top);
        await Clients.Group(inviteLink).AddCardRangeToPile([top]);

        _logger.LogInformation("Dealing cards to players.");

        // Deal player cards
        // TODO: Explicit card movements (Deck -> Hand, Hand -> Pile, etc).
        // Needs more thought: will have to restructure PerformTurn
        foreach (var hand in game.Hands)
        {
            const int count = 4;

            var dealt = deck.DealMany(count);
            await Clients.Group(inviteLink).RemoveCardsFromDeck(count);

            _logger.LogInformation("Dealt {Count} cards to {User}.", count, hand.User.UserName);
            _logger.LogInformation("Cards: {Cards}.", string.Join(", ", dealt));

            hand.Cards.AddRange(dealt);
            await Clients.User(hand.User.Id).AddCardRangeToHand(dealt);
            await Clients.Users(GetOthers(game.Hands, hand)).AddCardsToPlayerHand(hand.ToData(), count);
        }

        _logger.LogInformation("Done dealing cards.");

        // Start game
        game.IsStarted = true;
        await Clients.Group(inviteLink).UpdateGameStatus(true);
        await _context.SaveChangesAsync();
    }

    // TODO: Refactor this into a service (and add tests) so that it can be shared
    // with the API controller, gRPC service, and/or whatever else.
    public async Task PerformTurn(string roomId, List<Card> cards)
    {
        // Setup
        var room = await _context.Rooms
            .Include(r => r.Game)
            .ThenInclude(g => g.Hands)
            .ThenInclude(h => h.User)
            .SingleOrDefaultAsync(room => room.Id == Guid.Parse(roomId));

        if (room is null) return;

        var game = room.Game;
        var currentTurn = game.CurrentTurn;
        var player = await _users.FindByIdAsync(game.Hands[currentTurn].User.Id);
        if (player is null) return;

        var hand = player.Hands.Single(h => h.GameId == game.Id);
        var turn = new Turn { UserId = player.Id, Cards = cards };
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
            KarataEngine.EnsureTurnIsValid(game: game, cards: cards);
        }
        catch (TurnValidationException exception)
        {
            var message = new SystemMessage { Text = exception.Message, Type = MessageType.Error };
            await Clients.Caller.ReceiveSystemMessage(message);
            await Clients.Caller.NotifyTurnProcessed();
            return;
        }

        // Add cards to pile
        foreach (var card in cards) game.Pile.Push(card);
        await Clients.Group(roomId).AddCardRangeToPile(cards);

        // Remove cards from player hand
        await Clients.Caller.NotifyTurnProcessed();
        await Clients.Caller.RemoveCardRangeFromHand(cards);
        await Clients.Users(GetOthers(game.Hands, hand))
            .RemoveCardsFromPlayerHand(hand.ToData(), cards.Count);
        hand.Cards.RemoveAll(cards.Contains);

        // Generate delta and update game state.
        turn.Delta = KarataEngine.GenerateTurnDelta(game, cards);
        if (turn.Delta.RemoveRequestLevels > 0)
        {
            game.CurrentRequest = null;
            await Clients.Group(roomId).SetCurrentRequest(null);
        }

        if (turn.Delta.RequestLevel is not GameRequestLevel.NoRequest)
        {
            try
            {
                var specific = turn.Delta.RequestLevel is GameRequestLevel.CardRequest;
                turn.Request = await Clients.Caller.PromptCardRequest(specific);
                game.CurrentRequest = turn.Request;
                await Clients.Group(roomId).SetCurrentRequest(turn.Request);
            }
            catch (Exception ex)
            {
                _logger.LogError("This is what it is: {Message}", ex.Message);
            }
        }

        if (turn.Delta.Reverse) game.IsForward = !game.IsForward;
        (game.Give, game.Pick) = (turn.Delta.Give, turn.Delta.Pick);

        // Check whether there are cards to pick.
        if (game.Pick > 0)
        {
            // Remove card from pile
            if (!game.Deck.TryDealMany(game.Pick, out var dealt))
            {
                if (game.Pile.Count + game.Deck.Count - 1 > game.Pick)
                {
                    // Reclaim pile
                    foreach (var card in game.Pile.Reclaim()) game.Deck.Push(card);
                    await Clients.Group(roomId).ReclaimPile();

                    // Shuffle & deal
                    game.Deck.Shuffle();
                    dealt = game.Deck.DealMany(game.Pick);
                }
                else
                {
                    _logger.LogInformation(
                        "There are not enough cards to pick. Pile: {PileCount}, Deck: {DeckCount}, Pick: {PickCount}.",
                        game.Pile.Count,
                        game.Deck.Count,
                        game.Pick
                    );

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
            hand.Cards.AddRange(dealt);
            await Clients.Caller.AddCardRangeToHand(dealt);
            await Clients.Users(GetOthers(game.Hands, hand)).AddCardsToPlayerHand(hand.ToData(), dealt.Count);
            game.Pick = 0;
        }

        // Check whether the game is over.
        if (hand.Cards.Count == 0)
        {
            if (hand.IsLastCard && !cards[^1].IsSpecial())
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
        for (uint i = 0; i < turn.Delta.Skip; i++)
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
}