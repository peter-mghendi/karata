namespace Karata.Kit.Core.Exceptions;

public class KarataException : Exception
{
    protected KarataException() : base()
    {
    }

    public KarataException(string message) : base(message)
    {
    }
}