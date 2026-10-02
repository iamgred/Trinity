//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using TeamServer.Data;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.Models;

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
        /// Adds the specified agent to the underlying DbSet asynchronously.
        /// </summary>
        /// <remarks>The agent is tracked by the context but not persisted until SaveChangesAsync is
        /// called; database-generated values may be assigned when the entity is saved.</remarks>
        /// <param name="agent">Agent entity to add to the context.</param>
        /// <returns>A task that represents completion of the asynchronous add operation.</returns>
        public async System.Threading.Tasks.Task CreateAgentAsync(Agent agent)
        {
            await _dbSet.AddAsync(agent);
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
        /// Retrieves all agents from the database, including listener name, IP addresses, username, architecture,
        /// process details, last check-in, and sleep interval.
        /// </summary>
        /// <remarks>Uses Entity Framework Core to include the Listener navigation property and projects
        /// each entity to an AgentResponse; the query is executed asynchronously with ToListAsync().</remarks>
        /// <returns>A task representing the asynchronous operation whose result is a list of AgentResponse instances.</returns>
        public async Task<List<AgentResponse>> GetAgentsAsync()
        {
            return await _dbSet
                .Include(a => a.Listener)
                .Select(a => new AgentResponse(a.ID, a.Integrity, a.ExternalIP, a.InternalIP, a.Listener.Name, a.Username, a.Architecure, a.ProcesseName, a.ProcessPID, a.LastCheckIn, a.Sleep))
                .ToListAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets the agent with the specified check-in UUID.
        /// </summary>
        /// <remarks>Queries the underlying DbSet and returns the first agent whose CheckInUUID equals the
        /// specified value.</remarks>
        /// <param name="checkUUID">The check-in UUID to match.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the matching Agent, or null if
        /// no match is found.</returns>
        public async Task<Agent?> GetAgentByCheckInUUID(string checkUUID)
        {
            return await _dbSet
                .Where(a => a.CheckInUUID.Equals(checkUUID))
                .FirstOrDefaultAsync();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//