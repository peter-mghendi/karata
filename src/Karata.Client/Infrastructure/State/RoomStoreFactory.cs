using Karata.Cards;
using Karata.Shared.Models;

namespace Karata.Client.Infrastructure.State;

public class RoomStoreFactory(ILoggerFactory logging)
{
    public RoomState Create(RoomData room, Dictionary<string, List<Card>> cards)
        => new(
            room with
            {
                Game = room.Game with
                {
                    Hands = [..room.Game.Hands.Select(h => h with { Cards = cards[h.User.Id] })]
                }
            },
            logging
        );
}