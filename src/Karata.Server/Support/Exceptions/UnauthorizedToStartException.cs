namespace Karata.Server.Support.Exceptions;

public class UnauthorizedToStartException : KarataGameException
{
    public override string Message => "You do not have permission to start this game.";
}