using System.Collections.Generic;
using Karata.Cards;
using Karata.Web.Models;
using static Karata.Web.Models.Game;

namespace Karata.Web.Engines
{
    public interface IEngine
    {
        bool ValidateTurnCards(Game game, List<Card> turnCards);
        GameDelta GenerateTurnDelta(Game game, List<Card> turnCards);

        // TODO bool IsValidStartingCard(Card card);
        // TODO bool IsValidFinishingCard(Card card);
    }
}