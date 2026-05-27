using Karata.Kit.Domain.Models;

namespace Karata.Server.Services;

public static class Enrich
{
    public static RoomData ForUser(Room room, Hand me) => room.ToData() with { Game = ForUser(room.Game, me) };

    public static GameData ForUser(Game game, Hand me) => (GameData)game with
    {
        Hands =
        [
            ..from hand in game.Hands select hand.Id == me.Id
                ? hand.ToData() with { Cards = [..game.Hands.Single(h => h.Id == hand.Id).Cards] }
                : hand.ToData()
        ]
    };
}