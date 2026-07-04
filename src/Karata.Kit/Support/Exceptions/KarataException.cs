using Karata.Kit.Cards.Models;

namespace Karata.Kit.Support.Exceptions;

public static class KarataExceptionExtensions
{
    extension(KarataException exception)
    {
        public SystemMessage SystemMessage => new() { Text = exception.Message, Type = MessageType.Error };
    }
}

public class KarataException : Exception
{
    protected KarataException() : base()
    {
    }

    public KarataException(string message) : base(message)
    {
    }
}