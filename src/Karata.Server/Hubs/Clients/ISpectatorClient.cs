using Karata.Kit.Domain.Models;

namespace Karata.Server.Hubs.Clients;

public interface ISpectatorClient
{
    // Adds a user to the current room
    Task AddHandToRoom(long id, UserData user, HandStatus status);

    // Adds the current spectator to a room
    Task AddToRoom(RoomData room);
    
    // Advance the turn
    Task TurnCommitted(TurnResolution resolution);
    
    // Ends the game
    Task EndGame(GameResultData result);
    
    // Moves n cards from deck to a player's hand. 
    Task MoveCardsFromDeckToHand(long handId, List<Card> cards);
    
    // Moves cards from deck to the pile. 
    Task MoveCardsFromDeckToPile(List<Card> cards);
    
    // Moves cards from hand to the pile. 
    Task MoveCardsFromHandToPile(long handId, List<Card> cards, bool visible);
    
    // Receives a system message.
    Task SystemMessage(SystemMessage message);
    
    // Reclaims the pile and adds cards to the deck
    Task ReclaimPile();
    
    // Removes the current spectator from the room
    Task RemoveFromRoom();
    
    // Removes a player from the room
    Task RemoveHandFromRoom(long handId);
    
    // Updates the room administrator
    Task UpdateAdministrator(UserData administrator);
    
    // Updates the game status
    Task UpdateGameStatus(GameStatus status);
    
    // Updates hand status
    Task UpdateHandStatus(long handId, HandStatus status);
}