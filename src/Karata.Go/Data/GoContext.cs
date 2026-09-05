using Karata.Go.Models;
using Karata.Runtime.Data;
using Microsoft.EntityFrameworkCore;

namespace Karata.Go.Data;

public class GoContext(DbContextOptions<GoContext> options) : KarataContext<User>(options)
{
    public DbSet<Link> Links => Set<Link>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Link>().HasKey(x => x.Slug);
        modelBuilder.Entity<Link>()
            .HasOne(link => link.Creator)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict); 
    }
}