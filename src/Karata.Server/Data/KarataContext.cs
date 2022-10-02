using System.Text.Json;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;

namespace Karata.Server.Data;

public class KarataContext : ApiAuthorizationDbContext<User>
{
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Hand> Hands => Set<Hand>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Turn> Turns => Set<Turn>();

    public KarataContext(DbContextOptions<KarataContext> options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

        modelBuilder.Entity<Room>()
            .HasOne(r => r.Game)
            .WithOne()
            .HasForeignKey<Game>(g => g.RoomId);

        modelBuilder.Entity<Game>()
            .HasOne(r => r.Winner)
            .WithMany()
            .IsRequired(false);

        modelBuilder.Entity<Game>()
            .Property(g => g.CurrentRequest)
            .HasConversion(
                request => JsonSerializer.Serialize(request, options),
                json => JsonSerializer.Deserialize<Card>(json, options));

        modelBuilder.Entity<Game>()
            .Property(g => g.Deck)
            .HasConversion(
                deck => JsonSerializer.Serialize(deck.Reverse(), options),
                json => JsonSerializer.Deserialize<Deck>(json, options) ?? new(),
                new ValueComparer<Deck>(
                    (s1, s2) => s1 != null && s2 != null && s1.SequenceEqual(s2),
                    s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    s => new(s.Reverse())
                ));

        modelBuilder.Entity<Game>()
            .Property(g => g.Pile)
            .HasConversion(
                pile => JsonSerializer.Serialize(pile.Reverse(), options),
                json => JsonSerializer.Deserialize<Pile>(json, options) ?? new(),
                new ValueComparer<Pile>(
                    (s1, s2) => s1 != null && s2 != null && s1.SequenceEqual(s2),
                    s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    s => new(s.Reverse())
                ));

        modelBuilder.Entity<Hand>()
            .Property(h => h.Cards)
            .HasConversion(
                cards => JsonSerializer.Serialize(cards, options),
                json => JsonSerializer.Deserialize<List<Card>>(json, options) ?? new(),
                new ValueComparer<List<Card>>(
                    (s1, s2) => s1 != null && s2 != null && s1.SequenceEqual(s2),
                    s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    s => s.ToList()
                ));

        modelBuilder.Entity<Turn>()
            .Property(t => t.Request)
            .HasConversion(
                request => JsonSerializer.Serialize(request, options),
                json => JsonSerializer.Deserialize<Card>(json, options));

        modelBuilder.Entity<Turn>()
            .Property(t => t.Cards)
            .HasConversion(
                cards => JsonSerializer.Serialize(cards, options),
                json => JsonSerializer.Deserialize<List<Card>>(json, options),
                new ValueComparer<List<Card>>(
                    (s1, s2) => s1 != null && s2 != null && s1.SequenceEqual(s2),
                    s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    s => s.ToList()
                ));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.LogTo(Console.WriteLine);
        // optionsBuilder.EnableSensitiveDataLogging();
    }
}