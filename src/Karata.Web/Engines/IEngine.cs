using System.Collections.Generic;
using Karata.Cards;

namespace Karata.Web.Engines
{
    public interface IEngine
    {
        void ProcessPostTurnActions(Card topCard, List<Card> turnCards, out uint pickedCards);
        bool ValidateTurn(Card topCard, List<Card> turnCards);
    }
}