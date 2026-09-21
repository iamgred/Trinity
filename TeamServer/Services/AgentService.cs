//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.DTOs.Campaign;
using Trinity.Shared.Enums;

namespace TeamServer.Services
{
    public class AgentService
    {
        private DatabaseService _db;
        private static readonly Random _random = new Random();
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public AgentService(DatabaseService database)
        {
            this._db = database;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<AgentsDashboardDTO> GetAgentsDashboard()
        {
            var agents = await _db.GetAgentsAsync();
            var agentDTOs = agents
                .Select(a => new AgentDTO
                {
                    ID = a.ID,
                    CampaignID = (int)a.CampaignID,
                    IpV4 = GenerateRandomIPv4(),
                    IsActive = true,
                    OS = "Windows 11",
                    LastCheckIn = DateTime.Now,
                    Type = Util.GetEnumValue<AgentTypes>(a.Type)
                }
            ).ToList();

            var campaigns = await _db.GetCampaignsAsync();
            var campaignDTOs = campaigns
                .Select(c =>
                new CampaignDTO
                {
                    Name = c.Name,
                    Status = Util.GetEnumValue<CampaignStatuses>(c.Status)
                }
            ).ToList();

            return new AgentsDashboardDTO { Agents = agentDTOs, Campaigns = campaignDTOs };
        }


        private string GenerateRandomIPv4()
        {
            byte[] buffer = new byte[4];
            _random.NextBytes(buffer);
            IPAddress iP = new IPAddress(buffer);
            return iP.ToString();
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//