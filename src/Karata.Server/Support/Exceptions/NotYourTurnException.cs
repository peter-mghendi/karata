namespace Karata.Server.Support.Exceptions;

public class NotYourTurnException : KarataGameException
{
    public override string Message => "It is not your turn.";
}