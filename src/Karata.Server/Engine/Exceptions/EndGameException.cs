using Karata.Server.Exceptions;

namespace Karata.Server.Engine.Exceptions;

public class EndGameException(GameResult result) : KarataException
{
    public GameResult Result { get; } = result;
}