using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Moneta.Api.Realtime;

/// <summary>
/// Pure relay: the server pushes "ReceiveNotification" to connected clients (see
/// PostgresNotificationListener). Clients don't call back into the hub — no server-side
/// methods needed here.
/// </summary>
[Authorize]
public class NotificationsHub : Hub;
