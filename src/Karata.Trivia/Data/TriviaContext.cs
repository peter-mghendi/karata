using Karata.Runtime.Data;
using Karata.Trivia.Models;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Data;

public class TriviaContext(DbContextOptions<TriviaContext> options) : KarataContext<User>(options)
{
    public DbSet<Topic> Topics => Set<Topic>();

    public DbSet<Choice> Choices => Set<Choice>();

    public DbSet<Game> Games => Set<Game>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<NotificationSubscription> NotificationSubscriptions => Set<NotificationSubscription>();

    public DbSet<Question> Questions => Set<Question>();
    
    public DbSet<Response> Responses => Set<Response>();
    
    public DbSet<Round> Rounds => Set<Round>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasMany(user => user.CreatedGames)
            .WithOne(game => game.Creator)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Topic>()
            .HasMany(topic => topic.Games)
            .WithOne(game => game.Topic)
            .HasForeignKey(game => game.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Topic>()
            .HasMany(topic => topic.Questions)
            .WithOne(question => question.Topic)
            .HasForeignKey(question => question.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Topic>()
            .Property(topic => topic.Id)
            .HasMaxLength(50);
        modelBuilder.Entity<Topic>()
            .Property(topic => topic.Title)
            .HasMaxLength(50);
        modelBuilder.Entity<Topic>()
            .Property(topic => topic.Description)
            .HasMaxLength(1000);

        modelBuilder.Entity<Question>()
            .HasIndex(question => new { question.Id, TopicId = question.TopicId })
            .IsUnique();
        modelBuilder.Entity<Question>()
            .HasMany(question => question.Choices)
            .WithOne(choice => choice.Question)
            .HasForeignKey(choice => choice.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Question>()
            .Property(question => question.Text)
            .HasMaxLength(1000);
        
        modelBuilder.Entity<Choice>()
            .HasIndex(choice => new { choice.Id, choice.QuestionId })
            .IsUnique();
        modelBuilder.Entity<Choice>()
            .Property(choice => choice.Text)
            .HasMaxLength(1000);
        
        modelBuilder.Entity<Game>()
            .HasIndex(game => new { game.Id, game.TopicId })
            .IsUnique();
        modelBuilder.Entity<Game>()
            .HasMany(game => game.Players)
            .WithMany(user => user.ParticipatedGames)
            .UsingEntity<GameParticipant>(
                right => right
                    .HasOne(participant => participant.User)
                    .WithMany()
                    .HasForeignKey(participant => participant.UserId)
                    .OnDelete(DeleteBehavior.Restrict),
                left => left
                    .HasOne(participant => participant.Game)
                    .WithMany(game => game.Participants)
                    .HasForeignKey(participant => participant.GameId)
                    .OnDelete(DeleteBehavior.Restrict),
                join =>
                {
                    join.HasKey(participant => new
                    {
                        participant.GameId,
                        participant.UserId
                    });
                });

        modelBuilder.Entity<Round>()
            .HasIndex(round => new { round.Id, round.QuestionId })
            .IsUnique();
        modelBuilder.Entity<Round>()
            .HasIndex(round => new { round.GameId, round.Index })
            .IsUnique();
        modelBuilder.Entity<Round>()
            .HasIndex(round => new { round.GameId, round.QuestionId })
            .IsUnique();
        modelBuilder.Entity<Round>()
            .HasOne(round => round.Game)
            .WithMany(game => game.Rounds)
            .HasForeignKey(round => new { round.GameId, round.TopicId })
            .HasPrincipalKey(game => new { game.Id, game.TopicId })
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Round>()
            .HasOne(round => round.Question)
            .WithMany()
            .HasForeignKey(round => new { round.QuestionId, round.TopicId })
            .HasPrincipalKey(question => new { question.Id, question.TopicId })
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Response>().HasIndex(response => response.SubmittedAt);
        modelBuilder.Entity<Response>()
            .HasIndex(response => new { response.RoundId, response.UserId })
            .IsUnique();
        modelBuilder.Entity<Response>()
            .HasOne(response => response.User)
            .WithMany()
            .HasForeignKey(response => response.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Response>()
            .HasOne(response => response.Choice)
            .WithMany()
            .HasForeignKey(response => new { response.ChoiceId, response.QuestionId })
            .HasPrincipalKey(choice => new { choice.Id, choice.QuestionId })
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Response>()
            .HasOne(response => response.Round)
            .WithMany(round => round.Responses)
            .HasForeignKey(response => new { response.RoundId, response.QuestionId })
            .HasPrincipalKey(round => new { round.Id, round.QuestionId })
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Response>()
            .HasOne<GameParticipant>()
            .WithMany()
            .HasForeignKey(response => new { response.GameId, response.UserId })
            .HasPrincipalKey(participant => new { participant.GameId, participant.UserId })
            .OnDelete(DeleteBehavior.Restrict);
    }
}