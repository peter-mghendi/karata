namespace Karata.Server.Support.Exceptions;

public class InsufficientPlayersException : KarataGameException
{
    public override string Message => "This game requires 2-4 players.";
}