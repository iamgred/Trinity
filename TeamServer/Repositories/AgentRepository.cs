//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Data;
using Trinity.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace TeamServer.Repositories
{
    public class AgentRepository : RepositoryBase<Agent>
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of the AgentRepository class with the specified context.
        /// </summary>
        /// <remarks>Passes the provided context to the base class constructor.</remarks>
        /// <param name="context">The Context used for data access.</param>
        public AgentRepository(Context context) : base(context)
        {
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously retrieves the Agent with the specified identifier from the context.
        /// </summary>
        /// <remarks>Uses DbSet.FindAsync; may return a tracked entity from the context without issuing a
        /// database query.</remarks>
        /// <param name="ID">The identifier of the agent to retrieve.</param>
        /// <returns>The Agent with the specified identifier, or null if no matching entity is found.</returns>
        public async Task<Agent?> GetAgentByIDAsync(int ID)
        {
            return await _dbSet.FindAsync(ID);
        }
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously retrieves all Agent entities from the context.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Agent>> GetAgentsAsync()
        {
            List<Agent> result = await _context.Agents.ToListAsync();
            return result;
        }
        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<(Agent, string)> GetAgentAsync(int agentID)
        {
            Agent result = await _context.Agents.FirstOrDefaultAsync(a => a.ID.Equals(agentID)) ?? throw new InvalidOperationException("Agent not found");
            Campaign campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(result.CampaignID)) ?? throw new InvalidOperationException("Campaign not found");
            return (result, campaign.Name);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//