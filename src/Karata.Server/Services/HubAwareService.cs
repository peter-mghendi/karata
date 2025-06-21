using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public abstract class HubAwareService(IHubContext<GameHub, IGameClient> hub, Room room, User player, string client)
{
    protected readonly User CurrentPlayer = player;
    protected readonly string Client = client;
    protected readonly Room Room = room;

    protected Game Game => Room.Game;

    protected IGameClient Me => hub.Clients.User(CurrentPlayer.Id);
    protected IGameClient Prompt => hub.Clients.Client(Client);
    protected IGameClient Others => hub.Clients.Users(Keys(Game.HandsExceptPlayer(CurrentPlayer)));
    protected IGameClient Everyone => hub.Clients.Group(Room.Id.ToString());
    
    protected IGameClient Hand(Hand hand) => hub.Clients.User(hand.Player.Id);
    protected IGameClient HandsExcept(Hand hand) => hub.Clients.Users(Keys(Game.HandsExceptHand(hand)));

    protected async Task AddToRoom(string connection) =>
        await hub.Groups.AddToGroupAsync(connection, Room.Id.ToString());

    protected async Task RemoveFromRoom(string connection) =>
        await hub.Groups.RemoveFromGroupAsync(connection, Room.Id.ToString());

    private static List<string> Keys(HashSet<Hand> hands) => hands.Select(h => h.Player.Id).ToList();
}