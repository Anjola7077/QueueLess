using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace QueueLess.Api.Hubs;

[Authorize]
public class QueueHub : Hub
{
    public Task JoinQueueRoom(string queueId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"queue-{queueId}");
}
