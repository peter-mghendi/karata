using Karata.Trivia.Hubs.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Karata.Trivia.Hubs;

[Authorize]
public class NotificationHub : Hub<INotificationHubClient>;