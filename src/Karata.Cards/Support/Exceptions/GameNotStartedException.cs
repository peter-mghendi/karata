namespace Karata.Cards.Support.Exceptions;

public class GameNotStartedException : KarataGameException
{
    public override string Message => "This game has not yet started.";
}