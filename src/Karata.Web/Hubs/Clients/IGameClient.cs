using System.Threading.Tasks;
using Karata.Web.Models;

namespace Karata.Web.Hubs.Clients
{
    public interface IGameClient
    {
        Task ReceiveChatMessage(ChatMessage message);

        Task ReceiveSystemMessage(SystemMessage message);
    }
}