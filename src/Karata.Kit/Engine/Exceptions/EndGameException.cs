using Karata.Kit.Core.Exceptions;
using Karata.Kit.Domain.Models;

namespace Karata.Kit.Engine.Exceptions;

public class EndGameException(GameResultData result) : KarataException
{
    public GameResultData Result { get; } = result;
    
    public static EndGameException DeckExhaustion() => new(GameResultData.DeckExhaustion());
    public static EndGameException Win(UserData winner) => new(GameResultData.Win(winner));
}