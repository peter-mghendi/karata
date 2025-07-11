namespace Karata.Server.Support.Exceptions;

public class IncorrectPasswordException : PasswordException
{
    public override string Message => "The password you entered is incorrect.";
}