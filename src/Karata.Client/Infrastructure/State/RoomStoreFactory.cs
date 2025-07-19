using Karata.Cards;
using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomStoreFactory(ILoggerFactory logging)
{
    public RoomState Create(RoomData room, Dictionary<string, int> counts, List<Card> cards, string username)
        => new(room, counts, cards, username, logging);
}
