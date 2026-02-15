using Karata.Kit.Domain.Models;

namespace Karata.Server.Hubs.Clients;

public interface IReplayerClient
{
    // Adds a user to the current room
    Task AddHandToRoom(long id, UserData user, HandStatus status);

    // Adds the current spectator to a room
    Task AddToRoom(RoomData room);
    
    // Moves n cards from deck to a player's hand. 
    Task MoveCardsFromDeckToHand(long handId, List<Card> cards);
    
    // Moves cards from deck to the pile. 
    Task MoveCardsFromDeckToPile(List<Card> cards);
    
    // Moves cards from hand to the pile. 
    Task MoveCardsFromHandToPile(long handId, List<Card> cards, bool visible);
    
    // Receives a system message.
    Task ReceiveSystemMessage(SystemMessage message);
    
    // Reclaims the pile and adds cards to the deck
    Task ReclaimPile();
    
    // Removes the current spectator from the room
    Task RemoveFromRoom();
    
    // Removes a player from the room
    Task RemoveHandFromRoom(long handId);
    
    // Sets the current card request
    Task SetCurrentRequest(Card? request);
    
    // Updates the room administrator
    Task UpdateAdministrator(UserData administrator);
    
    // Updates the game status
    Task UpdateGameStatus(GameStatus status);
    
    // Updates hand status
    Task UpdateHandStatus(long handId, HandStatus status);
    
    // Updates the current pick value
    Task UpdatePick(uint num);
    
    // Updates the current turn
    Task UpdateTurn(int turn);
}