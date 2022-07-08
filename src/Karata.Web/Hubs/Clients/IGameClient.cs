#nullable enable

using Karata.Web.Models.UI;

namespace Karata.Web.Hubs.Clients;

public interface IGameClient
{
    Task AddCardsToPlayerHand(UIHand hand, int num);
    Task AddCardRangeToHand(List<Card> cards); 
    Task AddCardRangeToPile(List<Card> cards);
    Task AddHandToRoom(UIHand hand);
    Task AddToRoom(UIRoom room);
    Task EndGame(string reason, UIUser? winner);
    Task NotifyTurnProcessed();
    Task PromptCardRequest(bool specific);
    Task PromptLastCardRequest();
    Task ReceiveChat(UIChat message);
    Task ReceiveSystemMessage(SystemMessage message);
    Task ReclaimPile();
    Task RemoveCardsFromDeck(int num);
    Task RemoveCardRangeFromHand(List<Card> cards);
    Task RemoveCardsFromPlayerHand(UIHand hand, int num);
    Task RemoveFromRoom();
    Task RemoveHandFromRoom(UIHand hand);
    Task SetCurrentRequest(Card? request);
    Task UpdateGameStatus(bool started);
    Task UpdateTurn(int turn);
}