//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Checkin;
using Trinity.Shared.Interfaces;

namespace TeamServer.Services.Factories
{
    public class HostFactory : IHostFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public Trinity.Shared.Models.Host CreateHost(int agentID, ref IntialCheckInRequest metaData)
        {
            return new Trinity.Shared.Models.Host
            {
                AgentID = agentID,
                OS = metaData.OS,
                RAM = metaData.RAM,
                HostName = metaData.user,
                DiskSize = metaData.DiskSize,
                CPUCount = metaData.CPUCount,
                FreeDisk = metaData.FreeDisk,
                MACAddress = metaData.macAddress,
                Motherboard = metaData.motherboard
            };
            
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//