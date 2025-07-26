using Karata.Cards;
using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomStoreFactory(ILoggerFactory logging)
{
    public RoomState Create(RoomData room, Dictionary<string, List<Card>> cards, string username)
        => new(room, cards, username, logging);
}
