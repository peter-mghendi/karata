using Karata.Shared.Models;

namespace Karata.Server.Hubs.Clients;

public interface IPlayerClient
{
    // Adds another user to the current room
    Task AddHandToRoom(UserData user, HandStatus status);
    
    // Adds the current user to a room
    Task AddToRoom(RoomData room, Dictionary<string, List<Card>> cards);
    
    // Ends the game
    Task EndGame();
    
    // Moves n cards from deck to another player's hand. 
    Task MoveCardCountFromDeckToHand(UserData user, int num);
    
    // Moves cards from deck to current player's hand.
    Task MoveCardsFromDeckToHand(List<Card> cards);
    
    // Moves cards from deck to the pile. 
    Task MoveCardsFromDeckToPile(List<Card> cards);
    
    // Moves cards from hand to the pile. 
    Task MoveCardsFromHandToPile(UserData user, List<Card> cards);

    // Notifies the current user's client that the turn has been processed - so the UI can clean up and state.
    Task NotifyTurnProcessed();
    
    // Prompts for the user to select a card request.
    Task<Card?> PromptCardRequest(bool specific);
    
    // Prompts the user to declare last card status
    Task<bool> PromptLastCardRequest();
    
    // Prompts the user for a passcode.
    Task<string?> PromptPasscode();
    
    // Receives a chat message sent by another player.
    Task ReceiveChat(ChatData message);
    
    // Receives a system message.
    Task ReceiveSystemMessage(SystemMessage message);
    
    // Reclaims the pile and adds cards to the deck
    Task ReclaimPile();
    
    // Removes the current player from the room
    Task RemoveFromRoom();
    
    // Removes another player from the room
    Task RemoveHandFromRoom(UserData user);
    
    // Sets the current card request
    Task SetCurrentRequest(Card? request);
    
    // Updates the room administrator
    Task UpdateAdministrator(UserData administrator);
    
    // Updates the game status
    Task UpdateGameStatus(GameStatus status);
    
    // Removes another player from the room
    Task UpdateHandStatus(UserData user, HandStatus status);
    
    // Updates the current pick value
    Task UpdatePick(uint num);
    
    // Updates the current turn
    Task UpdateTurn(int turn);
}