namespace Karata.Server.Engine;

public class KarataEngineFactory
{
    public KarataEngine Create(Game game, List<Card> cards)
    {
        return new KarataEngine { Game = game, Cards = [..cards] };
    }
}