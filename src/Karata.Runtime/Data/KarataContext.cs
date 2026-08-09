using Karata.Runtime.Models;
using Microsoft.EntityFrameworkCore;

namespace Karata.Runtime.Data;

public class KarataContext<TUser>(DbContextOptions options) : DbContext(options) where TUser : KarataUser
{
    public DbSet<TUser> Users => Set<TUser>();
}