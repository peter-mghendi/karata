using Karata.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace Karata.Server.Services;

public class RoomService : IRoomService
{
    private readonly KarataContext _dbContext;

    public RoomService(KarataContext dbContext) => _dbContext = dbContext;

    public async Task<Room> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var room = new Room
        {
            InviteLink = Guid.NewGuid().ToString(),
            Creator = user,
        };
        _ = await _dbContext.Rooms.AddAsync(room, cancellationToken);
        return room;
    }

    public async Task<Room> FindByInviteLinkAsync(
        string inviteLink,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Rooms.SingleAsync(r => r.InviteLink == inviteLink, cancellationToken);
}