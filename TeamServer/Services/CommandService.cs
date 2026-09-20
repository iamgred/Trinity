//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Enums;
using Trinity.Shared.Interfaces;

namespace TeamServer.Services
{
    public class CommandService
    {
        private DatabaseService _db;
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public CommandService(DatabaseService database)
        {
            this._db = database;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<AsyncCommandResponseDTO> QueueTask(ICommand commandDTO, CommandTypes type, int agentID)
        {
            AsyncCommandResponseDTO response = new AsyncCommandResponseDTO();
            try
            {
                int taskID = await this._db.InsertAgentTask(commandDTO, type, agentID);
                response.TaskID = taskID;
            }
            catch (Exception ex) when (ex is InvalidOperationException)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//