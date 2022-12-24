namespace Karata.Server.Engine.Exceptions;

public class DrawCardsException : TurnValidationException
{
    public override string Message =>
        "You have pending cards to draw. You must either draw these cards, add to them or block them.";
}