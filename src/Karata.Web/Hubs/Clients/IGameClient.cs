#nullable enable

using Karata.Web.Models.UI;

namespace Karata.Web.Hubs.Clients;

public interface IGameClient
{
    Task AddCardsToDeck(int num);
    Task AddCardToHand(Card card);
    Task AddCardToPile(Card card);
    Task AddCardRangeToHand(List<Card> cards); 
    Task AddCardRangeToPile(List<Card> cards);
    Task AddHandToRoom(UIHand hand);
    Task AddToRoom(UIRoom room);
    Task EndGame(UIUser? winner);
    Task NotifyTurnProcessed(bool valid);
    Task PromptCardRequest(bool specific);
    Task PromptLastCardRequest();
    Task ReceiveChat(UIChat message);
    Task ReceiveSystemMessage(SystemMessage message);
    Task ReclaimPile();
    Task RemoveCardsFromDeck(int num);
    Task RemoveFromRoom();
    Task RemoveHandFromRoom(UIHand hand);
    Task SetCurrentRequest(Card? request);
    Task UpdateGameStatus(bool started);
    Task UpdateTurn(int turn);
}