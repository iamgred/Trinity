//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using TeamServer.Data;
using TeamServer.Utils;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.Models;
using Trinity.Shared.DTOs.Payload;

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
        public async Task<int> InsertAgentTask(ICommand command, CommandTypes type, int agentID)
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
                CommandType = type,
                Command = JsonDocument.Parse(JsonSerializer.Serialize(command, command.GetType())),
            };
            var result = await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return result.Entity.ID;
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
        public async Task<bool> ClearQueueAsync(int agentID)
        {
            var result = await _context.Tasks
                .Where(t => t.AgentID.Equals(agentID) && t.Status.Equals(TaskStatuses.Queued))
                .ExecuteDeleteAsync();

            return result > 0;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<int> InsertPayloadAsync(PayloadCreationDTO payloadCreationDTO)
        {
            Payload payload = new Payload
            {
                ListenerID = payloadCreationDTO.ListenerID,
                Architecture = Util.GetEnumString<Architectures>(payloadCreationDTO.Architecture),
                FileName = payloadCreationDTO.Name,
                CreatedAt = DateTime.UtcNow,
                PayloadType = Util.GetEnumString<PayloadTypes>(payloadCreationDTO.Type),
                Platform = Platforms.Windows,
                ProfileID = 1,
                UUID = "test"
            };

            var result = await _context.AddAsync(payload);
            await _context.SaveChangesAsync();
            return result.Entity.ID;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Payload>> GetPayloadsAsync()
        {
            List<Payload> result = await _context.Payloads.ToListAsync();
            return result;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Agent>> GetAgentsAsync() 
        {
            List<Agent> result = await _context.Agents.ToListAsync();
            return result;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<(Agent, string)> GetAgentAsync(int agentID)
        {
            Agent result = await _context.Agents.FirstOrDefaultAsync(a => a.ID.Equals(agentID));
            Campaign campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.ID.Equals(result.CampaignID));
            return (result, campaign.Name);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<Campaign>> GetCampaignsAsync()
        {
            List<Campaign> result = await _context.Campaigns.ToListAsync();
            return result;
        }

    }
}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //