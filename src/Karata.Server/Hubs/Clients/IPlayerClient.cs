using Karata.Kit.Domain.Models;

namespace Karata.Server.Hubs.Clients;

public interface IPlayerClient
{
    // Adds another user to the current room
    Task AddHandToRoom(Guid roomId, long id, UserData user, HandStatus status);
    
    // Adds the current user to a room
    Task AddToRoom(Guid roomId, RoomData room);
    
    // Receives a chat message sent by another player.
    Task Chat(Guid roomId, ChatData message);
    
    // Ends the game
    Task EndGame(Guid roomId, GameResultData result);
    
    // Moves cards from deck to current player's hand.
    Task MoveCardsFromDeckToHand(Guid roomId, long handId, List<Card> cards);
    
    // Moves cards from deck to the pile. 
    Task MoveCardsFromDeckToPile(Guid roomId, List<Card> cards);
    
    // Moves cards from hand to the pile. 
    Task MoveCardsFromHandToPile(Guid roomId, long handId, List<Card> cards, bool visible);
    
    // Prompts for the user to select a card request.
    Task<Card?> PromptCardRequest(Guid roomId, bool specific);
    
    // Prompts the user to declare last card status
    Task<bool> PromptLastCardRequest(Guid roomId);
    
    // Prompts the user for a passcode.
    Task<string?> PromptPasscode(Guid roomId);
    
    // Reclaims the pile and adds cards to the deck
    Task ReclaimPile(Guid roomId);
    
    // Removes the current player from the room
    Task RemoveFromRoom(Guid roomId);
    
    // Removes another player from the room
    Task RemoveHandFromRoom(Guid roomId, long handId);
    
    // Receives a system message.
    Task SystemMessage(Guid roomId, SystemMessage message);

    // Notifies the current user's client that the turn has been processed - so the UI can clean up and state.
    Task TurnAcknowledged(Guid roomId);
    
    // Advance the turn
    Task TurnCommitted(Guid roomId, GameData game);
    
    // Communicate rejection
    Task TurnRejected(Guid roomId, TurnValidationProblem problem);
    
    // Updates the room administrator
    Task UpdateAdministrator(Guid roomId, UserData administrator);
    
    // Updates the game status
    Task UpdateGameStatus(Guid roomId, GameData data);
    
    // Removes another player from the room
    Task UpdateHandStatus(Guid roomId, long handId, HandStatus status);
}