namespace Karata.Server.Support.Exceptions;

public class GameHasNotStartedException : KarataGameException
{
    public override string Message => "This game has not yet started.";
}