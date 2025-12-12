using Karata.Kit.Core.Exceptions;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Engine.Exceptions;

public class EndGameException(GameResultData result) : KarataException
{
    public GameResultData Result { get; } = result;
}