using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomStoreFactory(ILoggerFactory logging)
{
    public RoomState Create(RoomData room, string username) => new(room, username, logging);
}
