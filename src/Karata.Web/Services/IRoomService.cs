using System.Threading;
using System.Threading.Tasks;
using Karata.Web.Models;

namespace Karata.Web.Services
{
    public interface IRoomService
    {
        Task<Room> CreateAsync(Room room, CancellationToken cancellationToken = default);
        Task<Room> DeleteAsync(Room room, CancellationToken cancellationToken = default);
        Task<Room> FindByInviteLinkAsync(string inviteLink, CancellationToken cancellationToken = default);
        Task<Room> UpdateAsync(Room room, CancellationToken cancellationToken = default);
    }
}