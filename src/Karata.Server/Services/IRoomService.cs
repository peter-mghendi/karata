namespace Karata.Server.Services;

public interface IRoomService
{
    Task<Room> CreateAsync(User creator, CancellationToken cancellationToken = default);
    Task<Room> FindByInviteLinkAsync(string inviteLink, CancellationToken cancellationToken = default);
}