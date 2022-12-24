namespace Karata.Server.Engine.Exceptions;

public class InvalidCardSequenceException : TurnValidationException
{
    public override string Message =>
        "Invalid card sequence. Subsequent cards must either match the previous card's face or be answers.";
}