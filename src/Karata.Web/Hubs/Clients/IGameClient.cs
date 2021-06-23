using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Karata.Web.Models;

namespace Karata.Web.Hubs.Clients
{
    public interface IGameClient
    {
        Task ReceiveChatMessage(ChatMessage message);

        Task ReceiveSystemMessage(SystemMessage message);

        Task AddToRoom(Room room);

        Task RemoveFromRoom();

        Task UpdateGameInfo(Game game);

        Task TurnPerformed(User player);
    }
}