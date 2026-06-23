namespace Karata.Cards.Support.Exceptions;

public class InvalidTurnException : KarataGameException
{
    public override string Message => "This action cannot be performed in this turn.";
}