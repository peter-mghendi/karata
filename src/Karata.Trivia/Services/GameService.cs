using Karata.Kit.Platform;
using Karata.Kit.Trivia.Models;
using Karata.Trivia.Data;
using Karata.Trivia.Models;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Services;

public class GameService(Client platform,  TriviaContext context)
{
    public const int GameRounds = 7;

    // TODO: [Gap] Account for more than 2 players here
    // TODO: [Notifications] Send notification
    public async Task<Game> CreateGame(User creator, GameRequest request)
    {
        var topic = await context.Topics
            .Include(t => t.Questions)
            .SingleOrDefaultAsync(t => t.Id == request.TopicId);

        if (topic is null) throw new BadHttpRequestException("Topic not found.");

        var opponent = await FetchUserFromDatabase(request.OpponentId) ?? await FetchUserFromPlatform(request.OpponentId);
        if (opponent is null) throw new BadHttpRequestException("Opponent not found.");

        var rounds = topic.Questions
            .OrderBy(_ => Random.Shared.Next())
            .Take(GameRounds)
            .Select((question, index) => new Round { Index = index, Question = question })
            .ToList();

        var game = new Game
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Topic = topic,
            Creator = creator,
            Players = [creator, opponent],
            Rounds = rounds
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();
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