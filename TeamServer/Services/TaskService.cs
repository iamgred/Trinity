//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepo;
        private readonly AgentRepository _agentRepo;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of TaskService with the specified repositories.
        /// </summary>
        /// <remarks>Intended for constructor injection of required repositories.</remarks>
        /// <param name="taskRepository">Repository used to access and manage tasks.</param>
        /// <param name="agentRepository">Repository used to access and manage agents.</param>
        public TaskService(TaskRepository taskRepository, AgentRepository agentRepository)
        {
            _taskRepo = taskRepository;
            _agentRepo = agentRepository;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets queued tasks for the specified agent.
        /// </summary>
        /// <remarks>Returns an error Result (AgentError.NotFound) when no agent exists with the provided
        /// ID.</remarks>
        /// <param name="agentID">ID of the agent whose queued tasks are retrieved.</param>
        /// <returns>A Result<List<TaskResponse>> containing the agent's queued tasks, or an error Result if the agent is not
        /// found.</returns>
        public async Task<Result<List<TaskResponse>>> GetQueuedTasksByAgentID(int agentID)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);
            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }

            return await _taskRepo.GetQueuedAgentTasksByID(agentID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously retrieves all tasks assigned to agents.
        /// </summary>
        /// <remarks>Delegates retrieval to the task repository. The Result represents success or failure;
        /// callers should verify the Result before enumerating the returned list.</remarks>
        /// <returns>A Result containing a list of TaskResponse on success; contains error information on failure.</returns>
        public async Task<Result<List<TaskResponse>>> GetAllAgentTasks()
        {
            return await _taskRepo.GetAllAgentTasks();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Gets details for the task with the specified identifier.
        /// </summary>
        /// <remarks>Performs an asynchronous repository lookup; returns a NotFound result when no
        /// matching task exists.</remarks>
        /// <param name="taskID">The identifier of the task to retrieve.</param>
        /// <returns>A Result containing a GetTaskDetailsResponse when the task is found; otherwise a Result representing a
        /// NotFound error.</returns>
        public async Task<Result<GetTaskDetailsResponse>> GetTaskDetailsByIDAsync(int taskID)
        {
            var task = await _taskRepo.GetTaskDetailsByIDAsync(taskID);
            if (task == null)
            {
                return TaskError.NotFound(taskID);
            }
            return task;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Removes the task with the specified identifier and persists the change asynchronously.
        /// </summary>
        /// <remarks>Returns NotFound if no matching task exists; commits repository changes when removal
        /// succeeds.</remarks>
        /// <param name="taskID">Identifier of the task to remove.</param>
        /// <returns>A task that completes with a Result indicating success when the task is removed, or a NotFound error when no
        /// task with the specified identifier exists.</returns>
        public async Task<Result> RemoveTaskByIDAsync(int taskID)
        {
            var task = await _taskRepo.GetTaskByStatusAsync(taskID, "Queued");
            if (task == null)
            {
                return TaskError.NotFound(taskID);
            }
            _taskRepo.RemoveTask(task);
            await _taskRepo.CommitAsync();

            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Clears all tasks associated with the specified agent asynchronously.
        /// </summary>
        /// <remarks>Verifies the agent exists before clearing tasks and commits the changes to the task
        /// repository.</remarks>
        /// <param name="agentID">Identifier of the agent whose tasks will be cleared.</param>
        /// <returns>A Result indicating success, or an error result if the agent was not found.</returns>
        public async Task<Result> ClearAgentTasksAsync(int agentID)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);
            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }

            await _taskRepo.ClearTasksByAgentID(agentID);
            await _taskRepo.CommitAsync();
            return Result.Success();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//