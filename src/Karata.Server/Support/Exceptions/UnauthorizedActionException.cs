namespace Karata.Server.Support.Exceptions;

public class UnauthorizedActionException : KarataGameException
{
    public override string Message => "You do not have permission to perform this action.";
}