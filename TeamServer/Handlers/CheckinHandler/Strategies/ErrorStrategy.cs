//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace TeamServer.Handlers.CheckinHandler.Strategies
{
    public class ErrorStrategy : ICheckinStrategy
    {
        private readonly TaskRepository _taskRepo;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="taskRepository"></param>
        public ErrorStrategy(TaskRepository taskRepository)
        {
            _taskRepo = taskRepository;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async System.Threading.Tasks.Task ProcessResponseAsync(JsonElement element)
        {
            var taskID = element.GetProperty("TaskID").GetInt32();
            var task = await _taskRepo.GetByIdAsync(taskID) ?? throw new ArgumentNullException("Task does not exist!");

            var response = JsonSerializer.Deserialize<TaskResp<Trinity.Shared.DTOs.Task.Error>>(element);
            task.Status = Status.ERROR;
            task.Error = new Trinity.Shared.Models.Error { TaskID = response!.TaskID, ErrorCode = response!.Data.ErrorCode, Message = response!.Data.Message};

            await _taskRepo.CommitAsync();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//