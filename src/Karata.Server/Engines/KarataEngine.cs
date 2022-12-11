using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.GameRequestLevel;

namespace Karata.Server.Engines;

// This class should not interact with ApplicationUser or Room at all
public class KarataEngine : IEngine
{
    public bool ValidateTurnCards(Game game, List<Card> turnCards)
    {
        if (turnCards.Count == 0) return true;

        var topCard = game.Pile.Peek();
        var firstCard = turnCards[0];

        // If a card has been requested, that card - or an Ace - must start the turn.
        if (game.CurrentRequest is not null && firstCard is not { Face: Ace })
        {
            var request = game.CurrentRequest;
            if (request is not { Face: None } && !firstCard.FaceEquals(request)) return false;
            if (!firstCard.SuitEquals(request)) return false;
        }

        // If the top card is a "bomb", the next card should counter or block it.
        if (topCard.IsBomb() && game.Pick > 0 && firstCard is not { Face: Ace })
        {
            // Joker can only be countered by a joker while 2 and 3 can be countered by 2, 3 and Joker.
            if (topCard is { Face: Joker })
            {
                if (firstCard is not { Face: Joker }) return false;
            }
            else
            {
                if (!firstCard.IsBomb()) return false;
            }
        }
        
        var sequence = new List<Card>(turnCards).Prepend(topCard).ToList();
        for (var i = 1; i < sequence.Count; i++)
        {
            var thisCard = sequence[i];
            var prevCard = sequence[i - 1];

            // First card
            if (i == 1)
            {
                // Ace and joker go on top of anything
                if (thisCard is { Face: Ace or Joker }) continue;

                // Anything goes on top of an ace or joker
                if (prevCard is { Face: Ace or Joker }) continue;
                if (!thisCard.FaceEquals(prevCard) && !thisCard.SuitEquals(prevCard))
                    return false;
            }
            // Subsequent cards
            else
            {
                switch (thisCard)
                {
                    case { Face: Ace }:
                    {
                        // Ace, when not the first card, can only go on top of a question or another ace.
                        if (!prevCard.IsQuestion() && prevCard is not { Face: Ace })
                            return false;
                        break;
                    }
                    case { Face: Joker }:
                    {
                        // Joker, when not the first card, can only go on top of a question or another joker.
                        if (!prevCard.IsQuestion() && prevCard is not { Face: Joker })
                            return false;
                        break;
                    }
                    default:
                    {
                        // An answer is only valid if it is the same face or suit as the previous card.
                        if (prevCard.IsQuestion())
                        {
                            if (!thisCard.FaceEquals(prevCard) && !thisCard.SuitEquals(prevCard))
                                return false;
                        }
                        else
                        {
                            // If the previous card is not a question, the current card must be of the same face.
                            if (!thisCard.FaceEquals(prevCard))
                                return false;
                        }

                        break;
                    }
                }
            }
        }

        return true;
    }

    public GameDelta GenerateTurnDelta(Game game, List<Card> turnCards)
    {
        var delta = new GameDelta();

        if (turnCards.Count == 0)
        {
            // If the last card played is a "bomb" card, the player has to immediately pick the cards, otherwise they just pick one card
            delta.Pick = 1;
            if (game.CurrentRequest is not null) delta.RemoveRequestLevels = 0;
            if (game.Pick > 0) delta.Pick = game.Pick;
            return delta;
        }

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
            var totalAceValue = turnCards.Sum(card => card.GetAceValue());
            
            // Using aces to block any requests and setting delta.RemoveRequestLevels to the number of aces used, i.e:
            // - If three aces are played on top of a CardRequest(=2), 2 Aces are used.
            // - If one ace is played on top of a CardRequest(=2), 1 Ace is used.
            var remainingAceValue = totalAceValue - (long)game.RequestLevel;
            delta.RemoveRequestLevels = (uint)Math.Min(totalAceValue, (long)game.RequestLevel);

            // Using an ace to avoid picking cards
            if (game.Pick > 0) --remainingAceValue;

            // Any remaining ace value is added to the request level.
            if (remainingAceValue > 0) delta.RequestLevel = remainingAceValue > 1 ? CardRequest : SuitRequest;
        }

        // For an even number of "kickbacks", the current player plays again.
        var kingCount = turnCards.Count(card => card is { Face: King });
        if (kingCount is > 0 && kingCount % 2 == 0) delta.Skip = 0;

        return delta;
    }
}