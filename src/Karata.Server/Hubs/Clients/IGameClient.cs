using Karata.Shared.Models;

namespace Karata.Server.Hubs.Clients;

public interface IGameClient
{
    Task AddCardRangeToPile(List<Card> cards);
    Task AddHandToRoom(UserData user);
    Task AddToRoom(RoomData room);
    Task EndGame();
    
    // Moves n cards from deck to another player's hand. 
    Task MoveCardCountFromDeckToHand(UserData hand, int num);
    
    // Moves cards from deck to current player's hand.
    Task MoveCardsFromDeckToHand(List<Card> cards);

    Task NotifyTurnProcessed();
    Task<Card?> PromptCardRequest(bool specific);
    Task<bool> PromptLastCardRequest();
    Task ReceiveChat(ChatData message);
    Task ReceiveSystemMessage(SystemMessage message);
    Task ReclaimPile();
    [Obsolete] Task RemoveCardsFromDeck(int num);
    Task RemoveCardRangeFromHand(List<Card> cards);
    Task RemoveCardsFromPlayerHand(UserData user, int num);
    Task RemoveFromRoom();
    Task RemoveHandFromRoom(UserData user);
    Task SetCurrentRequest(Card? request);
    Task UpdateGameStatus(GameStatus status);
    Task UpdateTurn(int turn);
}