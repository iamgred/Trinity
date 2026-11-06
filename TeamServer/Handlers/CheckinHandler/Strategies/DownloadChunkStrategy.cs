//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using TeamServer.Interface;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;
using TeamServer.Services;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.DTOs.Notification;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;

namespace TeamServer.Handlers.CheckinHandler.Strategies
{
    public class DownloadChunkStrategy : ICheckinStrategy
    {
        private readonly IDownloadService _downloadService;
        private readonly CommandService _commandService;
        private readonly TaskRepository _taskRepo;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DownloadChunkStrategy(
            IDownloadService downloadService,
            CommandService commandService,
            TaskRepository taskRepo,
            IHubContext<NotificationHub> notificationHub)
        {
            _downloadService = downloadService;
            _commandService = commandService;
            _taskRepo = taskRepo;
            _hubContext = notificationHub;
        }

        public async System.Threading.Tasks.Task ProcessResponseAsync(JsonElement element)
        {
            if (!element.TryGetProperty("TaskID", out var taskIDElement) ||
                taskIDElement.ValueKind != JsonValueKind.Number ||
                !taskIDElement.TryGetInt32(out var taskID))
            {
                throw new InvalidOperationException(
                    "Download response is missing a valid TaskID.");
            }

            var task = await _taskRepo.GetByIdAsync(taskID) ?? throw new ArgumentNullException("Task does not exist!");

            var response = JsonSerializer.Deserialize<TaskResp<TaskSuccessResponse>>(element.GetRawText());

            if (response == null)
            {
                throw new InvalidOperationException("Download response could not be deserialized.");
            }

            task.Status = (Status)response.Status;

            await _taskRepo.CommitAsync();

            var originalDownload = task.Command.Deserialize<DownloadDTO>();

            if (originalDownload == null)
            {
                throw new InvalidOperationException("Original download command could not be deserialized.");
            }


            if (response.Status == (int)Status.DOWNLOAD_FINAL_STATUS)
            {
                await _hubContext.Clients.Group(task.AgentID.ToString()).SendAsync(
                    "TaskNotification",
                    new TaskNotification(
                        DateTime.UtcNow,
                        taskID,
                        $"Download completed successfully: {originalDownload.Path}",
                        Status.SUCCESS)
                );

                return;
            }

            if (response.Status != (int)Status.DOWNLOAD_CHUNK_STATUS)
            {
                throw new InvalidOperationException("Download response contains an invalid continuation status.");
            }

            var chunkSize =
                originalDownload.Data.Length > 0
                    ? Convert.FromBase64String(originalDownload.Data).LongLength
                    : 0;

            var nextOffset = originalDownload.Offset + chunkSize;

            var nextChunkResult =
                await _downloadService.DownloadChunkAsync(
                    originalDownload.Path,
                    originalDownload.DestinationPath,
                    nextOffset);

            if (!nextChunkResult.IsSuccess)
            {
                throw new InvalidOperationException("The next download chunk could not be created.");
            }

            var nextDownload = nextChunkResult.Response ??
                throw new InvalidOperationException("The next download chunk did not contain a response.");

            var nextTaskResult =
                await _commandService.QueueTaskAsync(
                    task.AgentID,
                    TaskType.DOWNLOAD,
                    nextDownload.Final
                        ? TaskCode.DOWNLOAD_FINAL
                        : TaskCode.DOWNLOAD_CHUNK,
                    nextDownload);

            if (!nextTaskResult.IsSuccess)
            {
                throw new InvalidOperationException("The next download task could not be queued.");
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//