using Karata.Platform.Models;
using Microsoft.EntityFrameworkCore;

namespace Karata.Platform.Data;

public class KarataPlatformContext(DbContextOptions<KarataPlatformContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
};