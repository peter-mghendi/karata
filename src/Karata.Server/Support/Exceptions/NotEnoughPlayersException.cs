namespace Karata.Server.Support.Exceptions;

public class NotEnoughPlayersException : KarataGameException
{
    public override string Message => "This game requires 2-4 players.";
}