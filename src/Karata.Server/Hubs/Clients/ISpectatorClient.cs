using Karata.Kit.Domain.Models;

namespace Karata.Server.Hubs.Clients;

public interface ISpectatorClient
{
    // Adds a user to the current room
    Task AddHandToRoom(Guid roomId, long id, UserData user, HandStatus status);

    // Adds the current spectator to a room
    Task AddToRoom(Guid roomId, RoomData room);
    
    // Ends the game
    Task EndGame(Guid roomId, GameResultData result);
    
    // Moves n cards from deck to a player's hand. 
    Task MoveCardsFromDeckToHand(Guid roomId, long handId, List<Card> cards);
    
    // Moves cards from deck to the pile. 
    Task MoveCardsFromDeckToPile(Guid roomId, List<Card> cards);
    
    // Moves cards from hand to the pile. 
    Task MoveCardsFromHandToPile(Guid roomId, long handId, List<Card> cards, bool visible);
    
    // Reclaims the pile and adds cards to the deck
    Task ReclaimPile(Guid roomId);
    
    // Removes the current spectator from the room
    Task RemoveFromRoom(Guid roomId);

    // Removes a player from the room
    Task RemoveHandFromRoom(Guid roomId, long handId);
    
    // Receives a system message.
    Task SystemMessage(Guid roomId, SystemMessage message);
    
    // Advance the turn
    Task TurnCommitted(Guid roomId, GameData game);
    
    // Updates the room administrator
    Task UpdateAdministrator(Guid roomId, UserData administrator);
    
    // Updates the game status
    Task UpdateGameStatus(Guid roomId, GameData game);
    
    // Updates hand status
    Task UpdateHandStatus(Guid roomId, long handId, HandStatus status);
}