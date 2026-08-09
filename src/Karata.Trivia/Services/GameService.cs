using Karata.Kit.Platform;
using Karata.Kit.Trivia.Models.Enum;
using Karata.Kit.Trivia.Models.Request;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Hubs;
using Karata.Trivia.Hubs.Clients;
using Karata.Trivia.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Services;

public class GameService(Client platform,  TriviaContext context, IHubContext<NotificationHub, INotificationHubClient> hub)
{
    public const int GameRounds = 7;

    public async Task<Game> CreateGame(User creator, CreateGameRequest request)
    {
        var topic = await context.Topics
            .Include(t => t.Questions)
            .SingleOrDefaultAsync(t => t.Id == request.TopicId);

        if (topic is null)
        {
            throw new BadHttpRequestException("Topic not found.");
        }

        var opponent = await FetchUserFromDatabase(request.OpponentId) ?? await FetchUserFromPlatform(request.OpponentId);
        if (opponent is null)
        {
            throw new BadHttpRequestException("Opponent not found.");
        }

        var rounds = topic.Questions
            .OrderBy(_ => Random.Shared.Next())
            .Take(GameRounds)
            .Select((question, index) => new Round { Index = index, Question = question })
            .ToList();

        var game = new Game
        {
            Identifier = Guid.CreateVersion7(),
            Topic = topic,
            PlayerOne = creator,
            PlayerTwo = opponent,
            Rounds = rounds
        };

        var notification = new Notification
        {
            Action = NotificationAction.Play,
            SentAt = DateTimeOffset.UtcNow,
            Game = game,
            Recipient = opponent
        };

        context.Games.Add(game);
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
        
        await context.Entry(opponent).Collection(u => u.NotificationSubscriptions).LoadAsync();
        await hub.Clients.User(opponent.Id).ReceiveNotification(notification.AsResponse());
        await PushNotificationService.SendNotificationAsync(notification);

        return game;
    }

    private async Task<User?> FetchUserFromDatabase(string id) => await context.Users.FindAsync(id);

    private async Task<User?> FetchUserFromPlatform(string id)
    {
        var profile = await platform.Profiles.GetAsync(id);
        if (profile is null) return null;
        
        var user = new User { Id = profile.Id, Username = profile.Username };
        var entity = context.Users.Add(user).Entity;
        await context.SaveChangesAsync();
        
        return entity;
    }
}