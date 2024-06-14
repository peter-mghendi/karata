using System.Text.Json;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;

namespace Karata.Server.Data;

public class KarataContext(
    DbContextOptions<KarataContext> options,
    IOptions<OperationalStoreOptions> operationalStoreOptions
) : ApiAuthorizationDbContext<User>(options, operationalStoreOptions)
{
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Hand> Hands => Set<Hand>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Turn> Turns => Set<Turn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        
        // Room
        modelBuilder.Entity<Room>().HasOne(r => r.Creator).WithMany();
        modelBuilder.Entity<Room>().HasOne(r => r.Game).WithOne().HasForeignKey<Game>(g => g.RoomId);
        modelBuilder.Entity<Room>().HasMany(r => r.Chats).WithOne();
        
        modelBuilder.Entity<Room>().Navigation(r => r.Creator).AutoInclude();
        modelBuilder.Entity<Room>().Navigation(r => r.Game).AutoInclude();
        modelBuilder.Entity<Room>().Navigation(r => r.Chats).AutoInclude();

        // Game
        modelBuilder.Entity<Game>().HasOne(g => g.Winner).WithMany().IsRequired(false);
        modelBuilder.Entity<Game>().HasMany(g => g.Hands).WithOne().HasForeignKey(h => h.GameId);
        modelBuilder.Entity<Game>().HasMany(g => g.Turns).WithOne().HasForeignKey(t => t.GameId);

        modelBuilder.Entity<Game>().OwnsOne(g => g.CurrentRequest, builder => builder.ToJson());
        // modelBuilder.Entity<Game>().OwnsOne(g => g.Deck, builder => builder.ToJson());
        // modelBuilder.Entity<Game>().OwnsOne(g => g.Pile, builder => builder.ToJson());
        
        modelBuilder.Entity<Game>()
            .Property(g => g.Deck)
            .HasConversion(
                deck => JsonSerializer.Serialize(deck.Reverse(), options),
                json => JsonSerializer.Deserialize<Deck>(json, options) ?? new(),
                new ValueComparer<Deck>(
                    (s1, s2) => s1 != null && s2 != null && s1.SequenceEqual(s2),
                    s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    s => new Deck(s.Reverse())
                ));

        modelBuilder.Entity<Game>()
            .Property(g => g.Pile)
            .HasConversion(
                pile => JsonSerializer.Serialize(pile.Reverse(), options),
                json => JsonSerializer.Deserialize<Pile>(json, options) ?? new Pile(),
                new ValueComparer<Pile>(
                    (s1, s2) => s1 != null && s2 != null && s1.SequenceEqual(s2),
                    s => s.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    s => new Pile(s.Reverse())
                ));
        
        modelBuilder.Entity<Game>().Navigation(g => g.Winner).AutoInclude();
        modelBuilder.Entity<Game>().Navigation(g => g.Hands).AutoInclude();
        modelBuilder.Entity<Game>().Navigation(g => g.Turns).AutoInclude();

        // Hand
        modelBuilder.Entity<Hand>().OwnsMany(h => h.Cards, builder => builder.ToJson());

        // Turn
        modelBuilder.Entity<Turn>().OwnsOne(t => t.Request, builder => builder.ToJson());
        modelBuilder.Entity<Turn>().OwnsMany(t => t.Cards, builder => builder.ToJson());
        modelBuilder.Entity<Turn>().OwnsOne(t => t.Delta, builder => builder.ToJson());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.LogTo(Console.WriteLine);
        // optionsBuilder.EnableSensitiveDataLogging();
    }
}