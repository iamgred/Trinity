//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;
using Trinity.Shared.DTOs.Notification;

namespace TeamServer.NotificationDispatchers
{
    public class NotificationHub : Hub
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Subscribes the client to a group related to the agentID.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public async Task SubscribeToAgent(string agentID)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, agentID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Sends the Agent task response to all subscribed terminal instances.
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        public async Task SendTaskResponseAsync(string agentID, TaskNotification notification)
        {
            await Clients.Group(agentID).SendAsync("TaskNotification", notification);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Unsubscribes a terminal instance from agent response updates.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public async Task UnsubscribeFromAgent(string agentID)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, agentID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Subscribes client to the specified campaign event log. 
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        public async Task SubscribeToEventLog(string campaignID)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, campaignID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Sends event log messages to event log to subscribed clients.
        /// </summary>
        /// <param name="campaignID"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public async Task SendEventLogAsync(string campaignID, EventLog eventLog)
        {
            await Clients.Group(campaignID).SendAsync("EventLogNotification", eventLog);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Unsubscribes client from the specified campaing event log.
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        public async Task UnsubscribeFromEventLog(string campaignID)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, campaignID);
        }
    }

}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//