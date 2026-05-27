using Karata.Kit.Domain.Models;

namespace Karata.Kit.Core.Exceptions;

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