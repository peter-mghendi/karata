namespace Karata.Web.Services;

public interface IUnitOfWork
{
    IRoomService RoomService { get; }

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}