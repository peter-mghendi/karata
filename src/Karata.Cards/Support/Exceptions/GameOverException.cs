namespace Karata.Cards.Support.Exceptions;

public class GameOverException : KarataGameException
{
    public override string Message => "This game is over.";
}