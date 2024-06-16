namespace Karata.Server.Support.Exceptions;

public class IncorrectPasswordException : KarataGameException
{
    public override string Message => "The password you entered is incorrect.";
}