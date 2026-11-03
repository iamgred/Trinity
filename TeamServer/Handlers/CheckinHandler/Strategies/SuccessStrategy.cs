//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace TeamServer.Handlers.CheckinHandler.Strategies
{
    public class SuccessStrategy : ICheckinStrategy
    {
        private readonly TaskRepository _taskRepo;
        private readonly NotificationHub _hubContext;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="taskRepository"></param>
        /// <param name="hubContext"></param>
        public SuccessStrategy(TaskRepository taskRepository, NotificationHub hubContext)
        {
            _taskRepo = taskRepository;
            _hubContext = hubContext;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Processes successful checkin task responses. 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async System.Threading.Tasks.Task ProcessResponseAsync(JsonElement element)
        {
            var taskID = element.GetProperty("TaskID").GetInt32();
            var task = await _taskRepo.GetByIdAsync(taskID) ?? throw new ArgumentNullException("Task does not exist!");

            var response = JsonSerializer.Deserialize<TaskResp<TaskSuccessResponse>>(element);
            if (response != null)
            {
                task.Status = Status.SUCCESS;
                task.Result = new TaskResult { TaskID = task.ID, Result = response.Data.Output };
                await _taskRepo.CommitAsync();
                await _hubContext.SendTaskResponseAsync(task.AgentID.ToString(), new TaskNotification(DateTime.UtcNow, taskID, response.Data.Output, Status.SUCCESS));
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//