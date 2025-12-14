namespace Karata.Server.Support.Exceptions;

public class SuspiciousCardsException : KarataGameException
{
    public override string Message => "The cards played do not match the current hand.";
}