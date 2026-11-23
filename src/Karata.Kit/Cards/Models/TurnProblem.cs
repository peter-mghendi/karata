using System.Collections.Immutable;
using Karata.Pips;

namespace Karata.Kit.Cards.Models;

public record TurnProblem
{
    public required int Index { get; init; }
    public required List<Card> Cards { get; init; }
    public required string Kind { get; init; }
    public required string Message { get; init; }

    public static TurnProblem CardRequested(int index, List<Card> cards) => new()
    {
        Index = index,
        Cards = cards,
        Kind = nameof(CardRequested),
        Message = "A card has been requested. Your turn must either start with the requested card or block the request."
    };

    public static TurnProblem DrawCards(int index, List<Card> cards) => new()
    {
        Index = index,
        Cards = cards,
        Kind = nameof(DrawCards),
        Message = "You have pending cards to draw. You must either draw these cards, add to them or block them."
    };

    public static TurnProblem InvalidAnswer(int index, List<Card> cards) => new()
    {
        Index = index,
        Cards = cards,
        Kind = nameof(InvalidAnswer),
        Message = "Invalid answer card. An answer is only valid if it is the same face or suit as the previous card."
    };

    public static TurnProblem InvalidSequence(int index, List<Card> cards) => new()
    {
        Index = index,
        Cards = cards,
        Kind = nameof(InvalidSequence),
        Message = "Invalid card sequence. Subsequent cards must either match the previous card's face or be answers."
    };

    public static TurnProblem InvalidFirstCard(int index, List<Card> cards) => new()
    {
        Index = index,
        Cards = cards,
        Kind = nameof(InvalidFirstCard),
        Message = "Invalid first card. The first card played must match the face or suit of the previous card."
    };

    public static TurnProblem SubsequentAceOrJoker(int index, List<Card> cards) => new()
    {
        Index = index,
        Cards = cards,
        Kind = nameof(SubsequentAceOrJoker),
        Message = "Invalid card order. An subsequent Ace/Joker can only go on top of a question or another Ace/Joker."
    };
}
