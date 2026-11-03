using System.Text.Json;
using TeamServer.Interface;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;
using SystemTask = System.Threading.Tasks.Task;

namespace TeamServer.Handlers.CheckinHandler.Strategies;

public sealed class UploadStrategy : ICheckinStrategy
{
    private readonly TaskRepository _taskRepository;
    private readonly IFileTransferService _fileTransferService;

    public UploadStrategy(TaskRepository taskRepository, IFileTransferService fileTransferService)
    {
        _taskRepository = taskRepository;
        _fileTransferService = fileTransferService;
    }

    public async SystemTask ProcessResponseAsync(JsonElement element)
    {
        var taskId = element.GetProperty("TaskID").GetInt32();
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new ArgumentException($"Task '{taskId}' does not exist.", nameof(element));
        var response = JsonSerializer.Deserialize<TaskResp<TaskFileChunkResponse>>(element)
            ?? throw new JsonException("The upload response was invalid.");

        await _fileTransferService.ReceiveCheckinUploadChunkAsync(
            taskId,
            response.Data.FileName,
            response.Data.Offset,
            response.Data.TotalBytes,
            response.Data.Data,
            response.Data.Final);

        if (response.Data.Final)
        {
            task.Status = Status.SUCCESS;
            task.Result = new TaskResult { TaskID = task.ID, Result = "File uploaded successfully." };
        }
        else
        {
            task.Command = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
                Path = task.Command.RootElement.GetProperty("Path").GetString(),
                Offset = response.Data.Offset + Convert.FromBase64String(response.Data.Data).LongLength,
                TotalBytes = response.Data.TotalBytes
            }));
            task.Status = Status.QUEUED;
        }

        await _taskRepository.CommitAsync();
    }
}
