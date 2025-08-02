using Karata.Shared.Exceptions;
using Karata.Shared.Models;

namespace Karata.Shared.Engine.Exceptions;

public class EndGameException(GameResultData result) : KarataException
{
    public GameResultData Result { get; } = result;
}