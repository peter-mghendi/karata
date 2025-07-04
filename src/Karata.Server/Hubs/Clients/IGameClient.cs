using System.Collections.Immutable;
using Karata.Shared.Models;

namespace Karata.Server.Hubs.Clients;

public interface IGameClient
{
    Task AddCardsToPlayerHand(UserData user, int num);
    Task AddCardRangeToHand(List<Card> cards); 
    Task AddCardRangeToPile(List<Card> cards);
    Task AddHandToRoom(UserData user);
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
    Task RemoveCardsFromPlayerHand(UserData user, int num);
    Task RemoveFromRoom();
    Task RemoveHandFromRoom(UserData user);
    Task SetCurrentRequest(Card? request);
    Task UpdateGameStatus(GameStatus status);
    Task UpdateTurn(int turn);
}