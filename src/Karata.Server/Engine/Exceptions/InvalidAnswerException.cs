namespace Karata.Server.Engine.Exceptions;

public class InvalidAnswerException : TurnValidationException
{
    public override string Message =>
        "Invalid answer card. An answer is only valid if it is the same face or suit as the previous card.";
}