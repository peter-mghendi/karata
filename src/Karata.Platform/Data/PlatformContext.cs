using Karata.Platform.Models;
using Karata.Runtime.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Karata.Platform.Data;

public class PlatformContext(DbContextOptions<PlatformContext> options) : KarataContext<User>(options)
{
    public DbSet<Activity> Activity => Set<Activity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.AccidentalEntityType));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(ActivityConfiguration.Instance);
    }
}