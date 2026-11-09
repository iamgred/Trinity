//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Notification;

namespace Trinity.Shared.Interfaces
{
    public interface INotificationDispatcher
    {
        Task SendTaskResponseAsync(string agentID, TaskNotification notification);
        Task SendEventLogAsync(string campaignID, EventLog eventLog);
    }

    public interface INotificationHub : INotificationDispatcher
    {
        Task SubscribeToAgent(string agentID);
        Task UnsubscribeFromAgent(string agentID);
        Task SubscribeToEventLog(string campaignID);
        Task UnsubscribeFromEventLog(string campaignID);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
