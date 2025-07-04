namespace Karata.Server.Support.Exceptions;

public class GameFullException : KarataGameException
{
    public override string Message => "This game is full.";
}