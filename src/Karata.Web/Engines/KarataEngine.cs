using System.Collections.Generic;
using System.Linq;
using Karata.Cards;
using Karata.Cards.Extensions;
using Karata.Web.Models;
using static Karata.Cards.Card.CardFace;
using static Karata.Cards.Card.CardSuit;
using static Karata.Web.Models.Game;

// TODO: Eventually move this out into its own classlib
// Use interfaces IGame, ITurn, IHand
// Concrete implementations for Card, Deck, Pile
// This class should not interact with ApplicationUser or Room at all
namespace Karata.Web.Engines
{
    public class KarataEngine : IEngine
    {
        // TODO: Configurable game rules.
        public bool ValidateTurnCards(Game game, List<Card> turnCards)
        {
            // Early exit
            if (turnCards.Count == 0) return true;

            var topCard = game.Pile.Peek();
            // var firstCard = turnCards[0];

            // If a card has been requested, that card must start the turn.
            // if (game.CurrentRequest is not null)
            // {
            //     var request = game.CurrentRequest;

            //     // Face is not none and not the same as the requested card.
            //     if (request.Face is not None && firstCard.Face != request.Face) return false;

            //     // Suit is not the same as the requested card.
            //     if (firstCard.Suit != request.Suit) return false;
            // }

            // If the top card is a "bomb", the next card should counter or block it.
            // if (topCard.IsBomb && game.Pick > 0 && firstCard is not { Face: Ace })
            // {
            //     // Joker can only be countered by a joker.
            //     if (topCard.IsJoker && !firstCard.IsJoker)
            //         return false;

            //     // Numeric "bomb" cards can only be countered by a card of equal or higher value.
            //     else if (firstCard.Rank > topCard.Rank)
            //         return false;
            // }

            // Everything, everything.
            var sequence = new List<Card>(turnCards).Prepend(topCard).ToList();

            for (int i = 1; i < sequence.Count; i++)
            {
                var thisCard = sequence[i];
                var prevCard = sequence[i - 1];

                // First card
                if (i == 1)
                {
                    // Joker as the first card allows any card after it.
                    // if (prevCard.IsJoker)
                    //     continue;

                    if (thisCard.Face != prevCard.Face && thisCard.Suit != prevCard.Suit)
                        return false;
                }
                // Subsequent cards
                else
                {
                    // Joker as the last card allows only question or joker before it.
                    // if (i == sequence.Count - 1 /* last card */
                    //     && thisCard.IsJoker
                    //     && prevCard.IsQuestion || prevCard.IsJoker)
                    //     continue;

                    if (prevCard.IsQuestion)
                    {
                        if (thisCard.Face != prevCard.Face && thisCard.Suit != prevCard.Suit)
                            return false;
                    }
                    else
                    {
                        if (thisCard.Face != prevCard.Face)
                            return false;
                    }
                }
            }

            // Happy path
            return true;
        }

        public GameDelta GenerateTurnDelta(Game game, List<Card> turnCards)
        {
            var delta = new GameDelta();

            // Everything, everything.
            // var topCard = game.Deck.Peek();
            // var sequence = new List<Card>(turnCards).Prepend(topCard).ToList();

            if (turnCards.Count == 0)
            {
                // If the last card played is a "bomb" card, the player has to immediately pick a card
                // if (game.Pick > 0)
                // {
                //     delta.Pick = game.Pick;
                //     return delta;
                // }

                delta.Pick = 1;
                return delta;
            }
            else
            {
                var lastCard = turnCards[^1];

                foreach (var card in turnCards)
                {
                    if (card is { Face: Jack }) ++delta.Skip;
                    if (card is { Face: King }) delta.Reverse = !delta.Reverse;
                }

                // If the last card played is a "question" card, the player has to immediately pick a card
                if (lastCard.IsQuestion)
                {
                    delta.Pick = 1;
                    return delta;
                }

                // if (lastCard.IsBomb)
                // {
                //     delta.Give = lastCard.IsJoker ? 5 : ((uint)lastCard.Face);
                //     return delta;
                // }

                //     if (lastCard.Face is Ace)
                //     {
                //         // TODO Handle card "requests"
                //     }
                // }

                // For an even number of kickbacks, the current player plays again.
                var kingCount = turnCards.Count(card => card is { Face: King });
                if (kingCount is > 0 && kingCount % 2 == 0) delta.Skip = 0;
            }

            return delta;
        }
    }
}