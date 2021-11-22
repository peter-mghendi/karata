#nullable enable

using Karata.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Karata.Web.Services;

public class RoomService : IRoomService
{
    private readonly ApplicationDbContext _dbContext;

    public RoomService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task CreateAsync(
        Room room,
        CancellationToken cancellationToken = default) =>
        _ = await _dbContext.Rooms.AddAsync(room, cancellationToken);

    public async Task<Room> FindByInviteLinkAsync(
        string inviteLink,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Rooms.SingleAsync(r => r.InviteLink == inviteLink, cancellationToken);
}