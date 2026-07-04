using Karata.Kit.Cards.Models;
using Karata.Kit.Support.Exceptions;

namespace Karata.Kit.Cards.Engine.Exceptions;

public class EndGameException(GameResultData result) : KarataException
{
    public GameResultData Result { get; } = result;
    
    public static EndGameException DeckExhaustion() => new(GameResultData.DeckExhaustion());
    public static EndGameException Win(UserData winner) => new(GameResultData.Win(winner));
}