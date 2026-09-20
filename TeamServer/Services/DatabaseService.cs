//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamServer.Data;
using Trinity.Shared.Interfaces;
namespace TeamServer.Services
{
    public class DatabaseService
    {
        private readonly Context _context;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initialises a new instance of the DatabaseService class.
        /// </summary>
        public DatabaseService(Context context)
        {
            this._context = context;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Inserts an Agent task.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public async Task<int> InsertAgentTask(ICommand command, int agentID)
        {
            bool agentExists = await _context.Agents.AnyAsync(a => a.ID.Equals(agentID));

            if (!agentExists)
            {
                throw new InvalidOperationException("Invalid: Operator does not exist!");
            }

            Trinity.Shared.Models.Task task = new Trinity.Shared.Models.Task
            {
                AgentID = agentID,
                CreatedAt = DateTime.UtcNow,
                Status = Trinity.Shared.Enums.TaskStatuses.Queued,
                Command = JsonDocument.Parse(JsonSerializer.Serialize(command)),
            };
            var result = await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return result.Entity.ID;
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //