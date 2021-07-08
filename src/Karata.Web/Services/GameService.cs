using System.Threading;
using System.Threading.Tasks;
using Karata.Web.Data;
using Karata.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Karata.Web.Services
{

    public class GameService : IGameService
    {
        private readonly ApplicationDbContext _dbContext;

        public GameService(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<Game> UpdateAsync(
            Game game,
            CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(game).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return game;
        }
    }
}