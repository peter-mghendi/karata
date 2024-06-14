using Karata.Server.Engine.Exceptions;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.GameRequestLevel;

namespace Karata.Server.Engine;

/// <summary>
/// <see cref="KarataEngine"/> has methods to check that the turn played is valid, and generate a
/// <see cref="GameDelta"/> for the turn.
/// </summary>
/// 
/// <remarks>
/// - This class should not interact with <see cref="User"/> or <see cref="Room"/>.
/// </remarks>
public static class KarataEngine
{
    /// <summary>
    /// <see cref="EnsureTurnIsValid"/> checks that the turn played is valid.
    /// </summary>
    /// 
    /// <exception cref="TurnValidationException">
    /// Thrown when the turn is not valid.
    /// </exception>
    public static void EnsureTurnIsValid(Game game, List<Card> cards)
    {
        if (cards.Count is 0) return; //An empty turn is always valid.

        var top = game.Pile.Peek();
        var first = cards[0];

        // If a card has been requested, that card - or an Ace - must start the turn.
        if (game.CurrentRequest is not null && first.Face is not Ace)
        {
            var request = game.CurrentRequest;
            if (request.Face is not None && !first.FaceEquals(request)) throw new CardRequestedException();
            if (!first.SuitEquals(request)) throw new CardRequestedException();
        }

        // If the top card is a "bomb", the next card should counter or block it.
        if (top.IsBomb() && game.Pick > 0 && first.Face is not Ace)
        {
            // Joker can only be countered by a joker while 2 and 3 can be countered by 2, 3 and Joker.
            if (top.Face is Joker)
            {
                if (first.Face is not Joker) throw new DrawCardsException();
            }
            else
            {
                if (!first.IsBomb()) throw new DrawCardsException();
            }
        }
        
        var sequence = new List<Card>(cards).Prepend(top).ToList();
        for (var i = 1; i < sequence.Count; i++)
        {
            var current = sequence[i];
            var previous = sequence[i - 1];

            // First card
            if (i is 1)
            {
                // Ace and joker go on top of anything
                if (current.Face is Ace or Joker) continue;

                // Anything goes on top of an ace or joker
                if (previous.Face is Ace or Joker) continue;
                
                // Otherwise, face or suit must match previous card.
                if (!current.FaceEquals(previous) && !current.SuitEquals(previous))
                {
                    throw new InvalidFirstCardException();
                }
            }
            // Subsequent cards
            else
            {
                switch (current)
                {
                    case { Face: Ace }:
                    {
                        // Ace, when not the first card, can only go on top of a question or another ace.
                        if (!previous.IsQuestion() && previous.Face is not Ace)
                        {
                            throw new SubsequentAceOrJokerException();
                        }
                        break;
                    }
                    case { Face: Joker }:
                    {
                        // Joker, when not the first card, can only go on top of a question or another joker.
                        if (!previous.IsQuestion() && previous.Face is not Joker)
                        {
                            throw new SubsequentAceOrJokerException();
                        }
                        break;
                    }
                    default:
                    {
                        // An answer is only valid if it is the same face or suit as the previous card.
                        if (previous.IsQuestion())
                        {
                            if (!current.FaceEquals(previous) && !current.SuitEquals(previous))
                            {
                                throw new InvalidAnswerException();
                            }
                        }
                        else
                        {
                            // If the previous card is not a question, the current card must be of the same face.
                            if (!current.FaceEquals(previous)) throw new InvalidCardSequenceException();
                        }

                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// <see cref="GenerateTurnDelta"/> Generates a <see cref="GameDelta"/> for this turn.
    /// </summary>
    public static GameDelta GenerateTurnDelta(Game game, List<Card> cards)
    {
        var delta = new GameDelta();

        if (cards.Count == 0)
        {
            // If the last card played is a "bomb" card, the player has to immediately pick the cards, otherwise they just pick one card
            delta.Pick = 1;
            if (game.CurrentRequest is not null) delta.RemoveRequestLevels = 0;
            if (game.Pick > 0) delta.Pick = game.Pick;
            return delta;
        }

        var last = cards[^1];
        foreach (var card in cards)
        {
            if (card.Face is Jack) ++delta.Skip;
            if (card.Face is King) delta.Reverse = !delta.Reverse;
        }

        // If the last card played is a "question" card, the player has to immediately pick a card
        if (last.IsQuestion())
        {
            return delta with { Pick = 1 };
        }

        // If the last card played is a "bomb" card, the next player should pick some cards.
        if (last.IsBomb())
        {
            return delta with { Give = last.GetPickValue() };
        }

        // If the last card played is an ace and nothing is being blocked, a card should be requested.
        if (last.Face is Ace)
        {
            // Getting ace values, instead of actual cards of the Ace suit, compensates for Ace of Spades (value = 2).
            var aces = cards.Sum(card => card.GetAceValue());
            
            // Using aces to block any requests and setting delta.RemoveRequestLevels to the number of aces used, i.e:
            // - If three aces are played on top of a CardRequest(=2), 2 Aces are used.
            // - If one ace is played on top of a CardRequest(=2), 1 Ace is used.
            delta.RemoveRequestLevels = (uint)Math.Min(aces, (long)game.RequestLevel);
            aces -= (long)game.RequestLevel;

            // Using an ace to avoid picking cards
            if (game.Pick > 0) --aces;

            // Any remaining ace value is added to the request level.
            if (aces > 0) delta.RequestLevel = aces > 1 ? CardRequest : SuitRequest;
        }

        // For an even number of "kickbacks", the current player plays again.
        var kings = cards.Count(card => card.Face is King);
        if (kings > 0 && kings % 2 == 0) delta.Skip = 0;

        return delta;
    }
}