//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;

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
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendTaskResponseAsync(string agentID, string message)
        {
            await Clients.Group(agentID).SendAsync(message);
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
        /// <param name="agentID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendEventLogAsync(string campaignID, string message)
        {
            await Clients.Group(campaignID).SendAsync(message);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Unsubscribes client from the specified campaing event log.
        /// </summary>
        /// <param name="campaignID"></param>
        /// <returns></returns>
        public async Task UnsubscribeFromEvenLog(string campaignID)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, campaignID);
        }
    }

}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//