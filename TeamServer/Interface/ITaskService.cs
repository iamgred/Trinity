// ============================================== { START OF FILE} ============================================== //
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface ITaskService
    {
        Task<Result<List<TaskResponse>>> GetQueuedTasksByAgentID(int agentID);
        Task<Result<List<TaskResponse>>> GetAllAgentTasks();
        Task<Result<GetTaskDetailsResponse>> GetTaskDetailsByIDAsync(int taskID);
        Task<Result> RemoveTaskByIDAsync(int taskID);
        Task<Result> ClearAgentTasksAsync(int agentID);
    }
}
// ============================================== { END OF FILE} ============================================== //