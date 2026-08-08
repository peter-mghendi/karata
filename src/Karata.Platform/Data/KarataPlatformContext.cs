using Karata.Platform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Karata.Platform.Data;

public class KarataPlatformContext(DbContextOptions<KarataPlatformContext> options) : DbContext(options)
{
    public DbSet<Activity> Activity => Set<Activity>();
    public DbSet<User> Users => Set<User>();

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