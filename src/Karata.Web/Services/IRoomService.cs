using System.Threading;
using System.Threading.Tasks;
using Karata.Web.Models;

namespace Karata.Web.Services
{
    public interface IRoomService
    {
        Task CreateAsync(Room room, CancellationToken cancellationToken = default);
        void Delete(Room room);
        Task<Room> FindByInviteLinkAsync(string inviteLink, CancellationToken cancellationToken = default);
        void Update(Room room);
    }
}