using System.Collections.Generic;
using System.Linq;
using Karata.Cards;
using static Karata.Cards.Card.CardFace;

namespace Karata.Web.Engines
{
    public class KarataEngine : IEngine
    {
        // TODO: Game state object
        public bool ValidateTurn(Card topCard, List<Card> turnCards)
        {
            // TODO: If a card has been requested, that card must start the turn.

            // Everything, everything.
            var sequence = new List<Card>(turnCards).Prepend(topCard).ToList();

            for (int i = 1; i < sequence.Count; i++)
            {
                Card thisCard = sequence[i];
                Card prevCard = sequence[i - 1];

                // A Joker should allow any card on either side
                if (thisCard.IsJoker || prevCard.IsJoker)
                    continue;

                // First card
                if (i == 1)
                {
                    if (thisCard.Face != prevCard.Face && thisCard.Suit != prevCard.Suit)
                        return false;
                }
                // Subsequent cards
                else
                {
                    // Previous card is not a question
                    if (!prevCard.IsQuestion)
                    {
                        if (thisCard.Face != prevCard.Face)
                            return false;
                    }
                    // Previous card is a question
                    else
                    {
                        if (thisCard.Face != prevCard.Face && thisCard.Suit != prevCard.Suit)
                            return false;
                    }
                }
            }

            // Happy path
            return true;
        }

        // TODO: Game state object
        public void ProcessPostTurnActions(Card topCard, List<Card> turnCards, out uint pickedCards)
        {
            pickedCards = 0;
            var lastCard = turnCards[^1];

            // If the last card played is a "question" card, the player has to immediately pick a card
            if (turnCards.Count == 0)
            {

            }
            else
            {
                // If the last card played is a "question" card, the player has to immediately pick a card
                if (lastCard.IsQuestion)
                {
                    pickedCards = 1;
                    return;
                }

                if (lastCard.IsBomb)
                {
                    pickedCards = lastCard.IsJoker ? 5 : ((uint)lastCard.Face);
                    return;
                }

                if (lastCard.Face is King)
                {
                    // TODO Handle game "kickbacks"
                }

                if (lastCard.Face is Jack)
                {
                    // TODO Handle player "jumps"
                }

                if (lastCard.Face is Ace)
                {
                    // TODO Handle card "requests"
                }
            }
        }
    }
}