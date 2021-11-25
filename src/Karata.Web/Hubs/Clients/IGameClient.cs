#nullable enable

namespace Karata.Web.Hubs.Clients;

public interface IGameClient
{
    Task AddCardsToDeck(uint num);
    Task AddCardToHand(Card card);
    Task AddCardToPile(Card card);
    Task AddCardRangeToHand(List<Card> cards); 
    Task AddCardRangeToPile(List<Card> cards);
    Task AddPlayerToRoom(ApplicationUser player);
    Task AddToRoom(Room room);
    Task EndGame(ApplicationUser? winner);
    Task EmptyHand();
    Task NotifyTurnProcessed(bool valid);
    Task PromptCardRequest(bool specific);
    Task PromptLastCardRequest();
    Task ReceiveChat(Chat message);
    Task ReceiveSystemMessage(SystemMessage message);
    Task ReclaimPile();
    Task RemoveCardsFromDeck(uint num);
    Task RemoveFromRoom();
    Task RemovePlayerFromRoom(ApplicationUser player);
    Task SetCurrentRequest(Card? request);
    Task UpdateGameStatus(bool started);
    Task UpdateTurn(int turn);
}