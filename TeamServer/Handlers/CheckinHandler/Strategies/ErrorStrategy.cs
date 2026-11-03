//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;

namespace TeamServer.Handlers.CheckinHandler.Strategies
{
    public class ErrorStrategy : ICheckinStrategy
    {
        private readonly TaskRepository _taskRepo;
        private readonly NotificationHub _hub;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="taskRepository"></param>
        public ErrorStrategy(TaskRepository taskRepository, NotificationHub notificationHub)
        {
            _taskRepo = taskRepository;
            _hub = notificationHub;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async System.Threading.Tasks.Task ProcessResponseAsync(JsonElement element)
        {
            var taskID = element.GetProperty("TaskID").GetInt32();
            var task = await _taskRepo.GetByIdAsync(taskID) ?? throw new ArgumentNullException("Task does not exist!");

            var response = JsonSerializer.Deserialize<TaskResp<Trinity.Shared.DTOs.Task.Error>>(element);

            if (response != null)
            {
                task.Status = Status.ERROR;
                task.Error = new Trinity.Shared.Models.Error { TaskID = response.TaskID, ErrorCode = response.Data.ErrorCode, Message = response.Data.Message };
                await _taskRepo.CommitAsync();
                await _hub.SendTaskResponseAsync(task.AgentID.ToString(), new TaskNotification(DateTime.UtcNow, taskID, response.Data.Message, Status.SUCCESS));
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//