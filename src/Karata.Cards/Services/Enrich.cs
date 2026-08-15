using Karata.Cards.Models;
using Karata.Kit.Cards.Models;

namespace Karata.Cards.Services;

public static class Enrich
{
    public static RoomData ForHand(Room room, Hand me) => room.ToData() with { Game = ForHand(room.Game, me) };

    public static GameData ForHand(Game game, Hand me) => (GameData)game with
    {
        Hands =
        [
            ..from hand in game.Hands select hand.Id == me.Id
                ? hand.ToData() with { Cards = [..game.Hands.Single(h => h.Id == hand.Id).Cards] }
                : hand.ToData()
        ]
    };
}