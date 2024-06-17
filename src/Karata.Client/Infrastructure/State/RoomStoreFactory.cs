using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomStoreFactory
{
    public RoomState Create(RoomData room, HandData hand) => new(room, hand);
}
