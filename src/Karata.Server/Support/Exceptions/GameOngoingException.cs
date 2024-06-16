namespace Karata.Server.Support.Exceptions;

public class GameOngoingException : KarataGameException
{
    public override string Message => "This game has already started.";
}