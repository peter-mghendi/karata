using Karata.Server.Hubs;
using Karata.Server.Hubs.Clients;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Server.Services;

public abstract class HubAwareService(IHubContext<GameHub, IGameClient> hub, Room room, string client)
{
    protected readonly Room Room = room;
    protected Game Game => Room.Game;
    protected Hand Hand => Game.CurrentHand;

    protected IGameClient Me => hub.Clients.User(Hand.Player.Id);
    protected IGameClient Prompt => hub.Clients.Client(client);
    protected IGameClient Others => hub.Clients.Users(Keys(Game.OtherHands));
    protected IGameClient Everyone => hub.Clients.Group(Room.Id.ToString());
    
    protected IGameClient Except(Hand hand) => hub.Clients.Users(Keys(Game.HandsExcept(hand)));

    protected async Task AddToRoom(Hand hand) =>
        await hub.Groups.AddToGroupAsync(hand.Player.Id, Room.Id.ToString());

    protected async Task RemoveFromRoom(Hand hand) =>
        await hub.Groups.RemoveFromGroupAsync(hand.Player.Id, Room.Id.ToString());

    private static List<string> Keys(HashSet<Hand> hands) => hands.Select(h => h.Player.Id).ToList();
}