//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.Interfaces;

namespace TeamServer.NotificationDispatchers
{
    public class NotificationDispatcher : INotificationHub
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationDispatcher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task SendTaskResponseAsync(string agentID, TaskNotification notification)
            => _hubContext.Clients.Group(agentID).SendAsync("TaskNotification", notification);

        public Task SendEventLogAsync(string campaignID, EventLog eventLog)
            => _hubContext.Clients.Group(campaignID).SendAsync("EventLogNotification", eventLog);

        // These are Hub methods invoked by connected SignalR clients, not by server-side services.
        public Task SubscribeToAgent(string agentID) => throw new NotSupportedException();
        public Task UnsubscribeFromAgent(string agentID) => throw new NotSupportedException();
        public Task SubscribeToEventLog(string campaignID) => throw new NotSupportedException();
        public Task UnsubscribeFromEventLog(string campaignID) => throw new NotSupportedException();
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
