using Karata.Shared.Models;

namespace Karata.Server.Hubs.Clients;

public interface ISpectatorClient
{
    // Adds a user to the current room
    Task AddHandToRoom(UserData user, HandStatus status);

    // Adds the current spectator to a room
    Task AddToRoom(RoomData room, Dictionary<string, int> counts);
    
    // Moves n cards from deck to a player's hand. 
    Task MoveCardCountFromDeckToHand(UserData user, int num);
    
    // Moves cards from deck to the pile. 
    Task MoveCardsFromDeckToPile(List<Card> cards);
    
    // Moves cards from hand to the pile. 
    Task MoveCardsFromHandToPile(UserData user, List<Card> cards);
    
    // Receives a system message.
    Task ReceiveSystemMessage(SystemMessage message);
    
    // Reclaims the pile and adds cards to the deck
    Task ReclaimPile();
    
    // Removes the current spectator from the room
    Task RemoveFromRoom();
    
    // Removes a player from the room
    Task RemoveHandFromRoom(UserData user);
    
    // Sets the current card request
    Task SetCurrentRequest(Card? request);
    
    // Updates the room administrator
    Task UpdateAdministrator(UserData administrator);
    
    // Updates the game status
    Task UpdateGameStatus(GameStatus status);
    
    // Updates hand status
    Task UpdateHandStatus(UserData user, HandStatus status);
    
    // Updates the current pick value
    Task UpdatePick(uint num);
    
    // Updates the current turn
    Task UpdateTurn(int turn);
}