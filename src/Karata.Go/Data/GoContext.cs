using Karata.Go.Models;
using Karata.Runtime.Data;
using Microsoft.EntityFrameworkCore;

namespace Karata.Go.Data;

public class GoContext(DbContextOptions<GoContext> options) : KarataContext<User>(options);