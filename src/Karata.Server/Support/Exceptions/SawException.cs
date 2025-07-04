namespace Karata.Server.Support.Exceptions;

public class SawException : KarataGameException
{
    public override string Message => "You cannot leave this room while the game is in progress.";
}