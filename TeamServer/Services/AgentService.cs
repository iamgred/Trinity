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


            return new();
        }

        public async Task<AgentDashboardDTO> GetAgentDashboard(int agentID) 
        {

            return new();
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