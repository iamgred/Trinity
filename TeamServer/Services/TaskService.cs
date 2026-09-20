//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.DTOs.Tasks;

namespace TeamServer.Services
{
    public class TaskService
    {
        private DatabaseService _db;
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public TaskService(DatabaseService database)
        {
            this._db = database;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<TaskDTO>> GetTasksAsync()
        {
            var tasks = await this._db.GetTasksAsync();

            var result = tasks.AsQueryable()
                .Select(t => new TaskDTO
                {
                    Status = Util.GetEnumValue(t.Status),
                    Command = Util.GetEnumValue(t.CommandType),
                    Created = t.CreatedAt
                })
                .ToList();

            return result;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<List<TaskDTO>> GetTasksByAgentAsync(int agentID)
        {
            var tasks = await this._db.GetTasksByAgentAsync(agentID);

            var result = tasks.AsQueryable()
                .Select(t => new TaskDTO
                {
                    Status = Util.GetEnumValue(t.Status),
                    Command = Util.GetEnumValue(t.CommandType),
                    Created = t.CreatedAt
                })
                .ToList();

            return result;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<bool> ClearAgentTasksAsync(int agentID)
        {
            bool result = await _db.ClearQueueAsync(agentID);
            return result;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//