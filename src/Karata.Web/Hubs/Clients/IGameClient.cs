using System.Collections.Generic;
using System.Threading.Tasks;
using Karata.Cards;
using Karata.Web.Models;

namespace Karata.Web.Hubs.Clients
{
    public interface IGameClient
    {
        Task AddCardsToDeck(int num);
        Task AddCardToHand(Card card);
        Task AddCardToPile(Card card);
        Task AddCardRangeToHand(List<Card> cards); 
        Task AddCardRangeToPile(List<Card> cards);
        Task AddPlayerToRoom(User player);
        Task AddToRoom(Room room);
        Task ReceiveChatMessage(ChatMessage message);
        Task ReceiveSystemMessage(SystemMessage message);
        Task ReclaimPile();
        Task RemoveCardsFromDeck(int num);
        Task RemoveFromRoom();
        Task RemovePlayerFromRoom(User player);
        Task UpdateGameStatus(bool started);
        Task UpdateTurn(int turn);
    }
}