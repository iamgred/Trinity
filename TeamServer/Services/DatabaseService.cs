//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TeamServer.Data;
using Trinity.Shared.DTOs.Tasks;
using Trinity.Shared.Models;
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
        public async Task<bool> InsertAgentTask(TaskDTO taskDTO, int agentID)
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
                Command = JsonDocument.Parse(JsonSerializer.Serialize(taskDTO.Command)),
            };
            await _context.Tasks.AddAsync(task);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Retrieves all the tasks.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Trinity.Shared.Models.Task>> GetTasksAsync() 
        {
            List<Trinity.Shared.Models.Task> tasks = await _context.Tasks.ToListAsync();

            return tasks;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Trinity.Shared.Models.Task?> GetTaskByIDAsync(int taskID)
        {
            Trinity.Shared.Models.Task? tasks = await _context.Tasks.FirstOrDefaultAsync(t => t.ID.Equals(taskID));

            return tasks;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Trinity.Shared.Models.Task>> GetTasksByAgentAsync(int agentID)
        {
            List<Trinity.Shared.Models.Task> tasks = await _context.Tasks
                .Where(t => t.AgentID.Equals(agentID))
                .ToListAsync();

            return tasks;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<bool> ClearQueue(int agentID)
        {
            var result = await _context.Tasks
                .Where(t => t.AgentID.Equals(agentID))
                .ExecuteDeleteAsync();

            return result > 0;
        }
    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //