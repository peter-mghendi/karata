namespace Karata.Server.Support.Exceptions;

public class PasswordRequiredException : KarataGameException
{
    public override string Message => "You need to enter a password to join this room.";
}