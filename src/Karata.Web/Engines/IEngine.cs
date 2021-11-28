#nullable enable

namespace Karata.Web.Engines;

public interface IEngine
{
    bool ValidateTurnCards(Game game, List<Card> turnCards);
    GameDelta GenerateTurnDelta(Game game, List<Card> turnCards);
}