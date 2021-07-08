using System.Threading;
using System.Threading.Tasks;
using Karata.Web.Models;

namespace Karata.Web.Services
{
    public interface IGameService
    {
        Task<Game> UpdateAsync(Game game, CancellationToken cancellationToken = default);
    }
}