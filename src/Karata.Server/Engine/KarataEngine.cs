using System.Collections.Immutable;
using Karata.Server.Engine.Exceptions;
using static Karata.Cards.Card.CardFace;
using static Karata.Server.Models.CardRequestLevel;

namespace Karata.Server.Engine;

/// <summary>
/// <see cref="KarataEngine"/> has methods to check that the turn played is valid, and generate a
/// <see cref="GameDelta"/> for the turn.
/// </summary>
/// 
/// <remarks>
/// - This class should not interact with <see cref="User"/> or <see cref="Room"/>.
/// </remarks>
public class KarataEngine
{
    public required Game Game { private get; init; }

    public required ImmutableArray<Card> Cards { private get; init; }

    /// <summary>
    /// <see cref="EnsureTurnIsValid"/> checks that the turn played is valid.
    /// </summary>
    /// 
    /// <exception cref="TurnValidationException">
    /// Thrown when the turn is not valid.
    /// </exception>
    public void EnsureTurnIsValid()
    {
        if (Cards.Length is 0) return; //An empty turn is always valid.

        var top = Game.Pile.Peek();
        var first = Cards.First();

        // If a card has been requested, that card - or an Ace - must start the turn.
        if (!RequestMatches(Game.Request, first) && first.Face is not Ace) throw new CardRequestedException();

        // If the top card is a "bomb", the next card should counter or block it.
        if (top.IsBomb() && Game.Pick > 0 && first.Face is not Ace)
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

        var sequence = new List<Card>(Cards).Prepend(top).ToImmutableArray();
        for (var i = 1; i < sequence.Length; i++)
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
    public GameDelta GenerateTurnDelta()
    {
        var delta = new GameDelta { Cards = Cards.ToList() };

        // If no cards are played,
        // - If the last card played is a "bomb" card that has not been picked, the player has to immediately pick the cards
        // - otherwise they just pick one card
        if (Cards.Length == 0) return delta with { Pick = Game.Pick > 0 ? Game.Pick : 1 };

        // If there was a request, and this turn is valid by virtue of the first card matching,
        // (i.e. not being an ace unless an Ace was requested), *clear* it immediately:
        // An Ace will still behave like an Ace if it was requested, its value will not be diminished.
        if (RequestMatches(Game.Request, Cards.First()))
        {
            delta = delta with { RemoveRequestLevels = (uint)Game.RequestLevel };
        }

        // Handle "jumps" and "kickbacks".
        delta = Enumerable.Aggregate(Cards, delta, (current, card) => card.Face switch
        {
            Jack => current with { Skip = current.Skip + 1 },
            King => current with { Reverse = !current.Reverse },
            _ => current
        });

        // If the last card played is a "question" card, the player has to immediately pick a card
        if (Cards.Last().IsQuestion()) return delta with { Pick = 1 };

        // If the last card played is a "bomb" card, the next player should pick some cards.
        if (Cards.Last().IsBomb()) return delta with { Give = Cards.Last().GetPickValue() };

        // If the last card played is an ace, handle two cases:
        //  - A non‐Ace request (standard behavior): knock off existing requests then any leftover aces become a new request
        //  - An Ace‐card request: clear the old request fully, then treat the ace play itself as a fresh request
        if (Cards.Last().Face is Ace)
        {
            // Getting ace values, instead of actual cards of the Ace suit, compensates for Ace of Spades (value = 2).
            var aces = Cards.Sum(card => card.GetAceValue());
            var removed = delta.RemoveRequestLevels;
            
            // Using aces to block any requests and setting delta.RemoveRequestLevels to the number of aces used, i.e:
            // - If three aces are played on top of a CardRequest(=2), 2 Aces are used.
            // - If one ace is played on top of a CardRequest(=2), 1 Ace is used.
            delta = delta with
            {
                RemoveRequestLevels = delta.RemoveRequestLevels + (uint)Math.Min(aces, (long)Game.RequestLevel - removed)
            };
            aces -= ((long)Game.RequestLevel - removed);

            // Using an ace to avoid picking cards
            if (Game.Pick > 0) --aces;

            // Any remaining ace value is added to the request level.
            if (aces > 0) delta = delta with { RequestLevel = aces > 1 ? CardRequest : SuitRequest };
        }

        // For an even number of "kickbacks", the current player plays again.
        var kings = Cards.Count(card => card.Face is King);
        if (kings > 0 && kings % 2 == 0) delta = delta with { Skip = 0 };

        return delta;
    }

    private static bool RequestMatches(Card? request, Card match) => request switch
    {
        null => true,
        { Face: None } => match.SuitEquals(request),
        { Face: not None } => match.SuitEquals(request) && match.FaceEquals(request),
    };
}