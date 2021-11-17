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
            var firstCard = turnCards[0];

            // If a card has been requested, that card must start the turn.
            // if (game.CurrentRequest is not null)
            // {
            //     var request = game.CurrentRequest;

            //     // Face is not none and not the same as the requested card.
            //     if (request.Face is not None && !firstCard.FaceEquals(firstCard)) return false;

            //     // Suit is not the same as the requested card.
            //     if (firstCard.Suit != request.Suit) return false;
            // }

            // If the top card is a "bomb", the next card should counter or block it.
            if (topCard.IsBomb() && game.Pick > 0 && firstCard is not { Face: Ace })
            {
                if (topCard is { Face: Joker })
                {
                    // Joker can only be countered by a joker.
                    if (firstCard is not { Face: Joker })
                        return false;
                }
                else
                {
                    // 2 and 3 can be countered by 2, 3 and Joker.
                    if (!firstCard.IsBomb()) return false;
                }
            }

            // Everything, everything.
            var sequence = new List<Card>(turnCards).Prepend(topCard).ToList();

            for (int i = 1; i < sequence.Count; i++)
            {
                var thisCard = sequence[i];
                var prevCard = sequence[i - 1];

                // First card
                if (i == 1)
                {
                    // Ace and joker go on top of anything
                    if (thisCard is { Face: Ace or Joker }) continue;

                    // Anything goes on top of an ace or joker
                    if (prevCard is not { Face: Ace or Joker })
                    {
                        if (!thisCard.FaceEquals(prevCard) && !thisCard.SuitEquals(prevCard))
                            return false;
                    }
                }
                // Subsequent cards
                else
                {
                    if (thisCard is { Face: Ace })
                    {
                        if (!prevCard.IsQuestion() && prevCard is not { Face: Ace })
                            return false;
                    }
                    else if (thisCard is { Face: Joker })
                    {
                        if (!prevCard.IsQuestion() && prevCard is not { Face: Joker })
                            return false;
                    }
                    else
                    {
                        if (prevCard.IsQuestion())
                        {
                            if (!thisCard.FaceEquals(prevCard) && !thisCard.SuitEquals(prevCard))
                                return false;
                        }
                        else
                        {
                            if (!thisCard.FaceEquals(prevCard))
                                return false;
                        }
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
                delta.Pick = 1;

                // If the last card played is a "bomb" card, the player has to immediately pick cards.
                if (game.Pick > 0) delta.Pick = game.Pick;
                
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
                if (lastCard.IsQuestion())
                {
                    delta.Pick = 1;
                    return delta;
                }

                // If the last card played is a "bomb" card, the next player should pick some cards.
                if (lastCard.IsBomb())
                {
                    delta.Give = lastCard.GetPickValue();
                    return delta;
                }

                // If the last card played is an ace and nothing is being blocked, a card should be requested.
                if (lastCard is { Face: Ace })
                {
                    var aceValueCount = turnCards.Sum(card => card.GetAceValue());
                    if (game.Pick > 0) --aceValueCount;

                    if (aceValueCount > 0) 
                    {
                        delta.HasRequest = true;
                        if (aceValueCount > 1) delta.HasSpecificRequest = true;
                    }                    
                }

                // For an even number of "kickbacks", the current player plays again.
                var kingCount = turnCards.Count(card => card is { Face: King });
                if (kingCount is > 0 && kingCount % 2 == 0) delta.Skip = 0;
            }

            return delta;
        }
    }
}