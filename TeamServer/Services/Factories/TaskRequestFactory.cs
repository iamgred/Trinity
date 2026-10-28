//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Interfaces;

namespace TeamServer.Services.Factories
{
    public class TaskRequestFactory : ITaskRequestFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a task request DTO in json format.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public string Create(Trinity.Shared.Models.Task? task)
            => Create(task, null);

        public string Create(Trinity.Shared.Models.Task? task, JsonDocument? commandOverride)
        {
            return task != null ? JsonSerializer.Serialize(
                new TaskRequest
                {
                    TaskID = task.ID,
                    TaskType = (int)task.CommandType,
                    TaskCode = (int)task.Code,
                    Command = commandOverride ?? task.Command,
                    Status = (int)task.Status
                }
            ) : string.Empty;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//