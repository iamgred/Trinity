//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Checkin;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace TeamServer.Services.Factories
{
    public class AgentFactory : IAgentFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates an Agent instance initialized from the provided IntialCheckInRequest.
        /// </summary>
        /// <remarks>CheckInUUID is generated via new Guid().ToString(); LastCheckIn is set to
        /// DateTime.UtcNow. Other fields are assigned default or fixed values (Architecure = "x86", CampaignID = 1,
        /// PayloadID = 1, AES256Key = "testkey", Sleep = 5000, Jitter = 10, ListenerID = 1).</remarks>
        /// <param name="request">Initial check-in request containing agent metadata such as user, processName, PID, externalIP, internalIP,
        /// and Integrity.</param>
        /// <returns>An Agent populated from the request and default configuration values.</returns>
        public Agent Create(IntialCheckInRequest request)
        {
            return new Agent
            {
                Username = request.user,
                Architecure = "x86",
                CampaignID = 1,
                ProcesseName = request.processName,
                ProcessPID = request.PID,
                CheckInUUID = new Guid().ToString(),
                PayloadID = 1,
                AES256Key = "testkey",
                LastCheckIn = DateTime.UtcNow,
                ExternalIP = request.externalIP,
                InternalIP = request.internalIP,
                Sleep = 5000,
                Jitter = 10,
                Integrity = request.Integrity,
                ListenerID = 1
            };
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//