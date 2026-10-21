using System.Net.Http.Headers;
using Karata.Kit.Cards.Services;
using Karata.Kit.Support.Http;
using RestSharp;
using RestSharp.Interceptors;

namespace Karata.Kit.Cards;

public sealed class Client(Client.Options options) : BaseClient<Client.Options>(options)
{
    public sealed class Options : IClientOptions
    {
        public required Uri Host { get; set; }

        public required Func<Task<string?>> TokenProvider { get; set; }
    }
    
    public RoomHandsService RoomHands => new(Client);

    public RoomService Rooms => new(Client);

    public TurnService Turns => new(Client);
}