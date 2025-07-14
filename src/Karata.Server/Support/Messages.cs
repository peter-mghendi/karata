using Karata.Server.Exceptions;
using Karata.Shared.Models;

namespace Karata.Server.Support;

public static class Messages
{
    public static SystemMessage Cardless(User player) => 
        new() { Text = $"{player.UserName} is cardless.", Type = MessageType.Info };
    
    public static SystemMessage Exception(KarataException exception) => 
        new() { Text = exception.Message, Type = MessageType.Error };
    
    public static SystemMessage GameOver(GameResult result) => 
        new() { Text = result.Reason, Type = result.ReasonType };
    
    public static SystemMessage LastCard(User player) => 
        new() { Text = $"{player.UserName} is on their last card.", Type = MessageType.Warning };
}