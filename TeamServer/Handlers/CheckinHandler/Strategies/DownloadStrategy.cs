using System.Text.Json;
using TeamServer.Repositories;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums.Tasks;

namespace TeamServer.Handlers.CheckinHandler.Strategies;

public sealed class DownloadStrategy : ICheckinStrategy
{
    private readonly TaskRepository _taskRepository;

    public DownloadStrategy(TaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task ProcessResponseAsync(JsonElement element)
    {
        var taskId = element.GetProperty("TaskID").GetInt32();
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task?.CommandType != TaskType.DOWNLOAD || task.Code != TaskCode.DOWNLOAD)
            return;

        var status = (Status)element.GetProperty("status").GetInt32();
        var final = status == Status.DOWNLOAD_FINAL;
        if (final)
        {
            task.Status = Status.SUCCESS;
            task.Result = new Trinity.Shared.Models.TaskResult
            {
                TaskID = task.ID,
                Result = "File downloaded successfully."
            };
        }
        else
        {
            task.Status = Status.QUEUED;
        }
        await _taskRepository.CommitAsync();
    }
}
