using System.Text.Json;
using TeamServer.Repositories;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums.Tasks;

namespace TeamServer.Handlers.CheckinHandler.Strategies;

public sealed class DownloadStrategy : ICheckinStrategy
{
    private readonly SuccessStrategy _successStrategy;
    private readonly TaskRepository _taskRepository;

    public DownloadStrategy(SuccessStrategy successStrategy, TaskRepository taskRepository)
    {
        _successStrategy = successStrategy;
        _taskRepository = taskRepository;
    }

    public async Task ProcessResponseAsync(JsonElement element)
    {
        await _successStrategy.ProcessResponseAsync(element);

        var taskId = element.GetProperty("TaskID").GetInt32();
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task?.CommandType != TaskType.DOWNLOAD || task.Code != TaskCode.DOWNLOAD)
            return;

        var final = task.Command.RootElement.TryGetProperty("Final", out var finalElement)
            && finalElement.GetBoolean();
        if (!final)
        {
            task.Status = Status.QUEUED;
            await _taskRepository.CommitAsync();
        }
    }
}
