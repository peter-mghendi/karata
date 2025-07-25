namespace Karata.Server.Support.Exceptions;

public class PasswordRequiredException : PasswordException
{
    public override string Message => "You need to enter a password to join this room.";
}