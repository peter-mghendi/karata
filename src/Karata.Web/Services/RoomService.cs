using System.Threading;
using System.Threading.Tasks;
using Karata.Web.Data;
using Karata.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Karata.Web.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _dbContext;

        public RoomService(ApplicationDbContext dbContext) => _dbContext = dbContext;

        public async Task<Room> CreateAsync(
            Room room,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Rooms.AddAsync(room, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return room;
        }

        public async Task<Room> UpdateAsync(
            Room room,
            CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(room).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return room;
        }

        public async Task<Room> DeleteAsync(
            Room room,
            CancellationToken cancellationToken = default)
        {
            _dbContext.Rooms.Remove(room);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return room;
        }

        public async Task<Room> FindByInviteLinkAsync(
            string inviteLink,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Rooms.SingleAsync(r => r.InviteLink == inviteLink, cancellationToken);
        }
    }
}