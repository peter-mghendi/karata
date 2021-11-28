#nullable enable

using Karata.Web.Data;

namespace Karata.Web.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly KarataContext _context;
    public IRoomService RoomService { get; }

    public UnitOfWork(KarataContext context, IRoomService roomService)
    {
        _context = context;
        RoomService = roomService;
    }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}