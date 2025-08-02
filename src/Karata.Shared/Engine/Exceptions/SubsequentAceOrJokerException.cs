namespace Karata.Shared.Engine.Exceptions;

public class SubsequentAceOrJokerException : TurnValidationException
{
    public override string Message =>
        "Invalid card order. An subsequent Ace/Joker can only go on top of a question or another Ace/Joker.";
}