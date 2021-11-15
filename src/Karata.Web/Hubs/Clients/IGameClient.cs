using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Karata.Cards;
using Karata.Web.Models;

namespace Karata.Web.Hubs.Clients
{
    public interface IGameClient
    {
        Task AddCardsToDeck(uint num);
        Task AddCardToHand(Card card);
        Task AddCardToPile(Card card);
        Task AddCardRangeToHand(List<Card> cards); 
        Task AddCardRangeToPile(List<Card> cards);
        Task AddPlayerToRoom(ApplicationUser player);
        Task AddToRoom(Room room);
        Task EmptyHand();
        Task NotifyInvalidTurn();
        Task NotifyValidTurn();
        Task PromptCardRequest(Guid identifier);
        Task ReceiveChat(Chat message);
        Task ReceiveSystemMessage(string message);
        Task ReclaimPile();
        Task RemoveCardsFromDeck(uint num);
        Task RemoveFromRoom();
        Task RemovePlayerFromRoom(ApplicationUser player);
        Task UpdateGameStatus(bool started);
        Task UpdateTurn(int turn);
    }
}