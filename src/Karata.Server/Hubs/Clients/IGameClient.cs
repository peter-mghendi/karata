using Karata.Shared.Models;

namespace Karata.Server.Hubs.Clients;

public interface IGameClient
{
    Task AddCardsToPlayerHand(HandData hand, int num);
    Task AddCardRangeToHand(List<Card> cards); 
    Task AddCardRangeToPile(List<Card> cards);
    Task AddHandToRoom(HandData hand);
    Task AddToRoom(RoomData room);
    Task EndGame();
    Task NotifyTurnProcessed();
    Task<Card?> PromptCardRequest(bool specific);
    Task<bool> PromptLastCardRequest();
    Task ReceiveChat(ChatData message);
    Task ReceiveSystemMessage(SystemMessage message);
    Task ReclaimPile();
    Task RemoveCardsFromDeck(int num);
    Task RemoveCardRangeFromHand(List<Card> cards);
    Task RemoveCardsFromPlayerHand(HandData hand, int num);
    Task RemoveFromRoom();
    Task RemoveHandFromRoom(HandData hand);
    Task SetCurrentRequest(Card? request);
    Task UpdateGameStatus(bool started);
    Task UpdateTurn(int turn);
}