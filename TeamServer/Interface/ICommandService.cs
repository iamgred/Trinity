using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface ICommandService
    {
        Task<Result> QueueTaskAsync(
            int agentID,
            TaskType type,
            TaskCode code,
            ICommand command);

        Task<Result> QueueNetAsmTask(int agentID, TaskType type, TaskCode code, DotNetAsmDTO command);
    }
}