namespace Karata.Shared.Engine.Exceptions;

public class InvalidFirstCardException : TurnValidationException
{
    public override string Message =>
        "Invalid first card. The first card played must match the face or suit of the previous card.";
}