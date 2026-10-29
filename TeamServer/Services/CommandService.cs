//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class CommandService
    {
        private readonly CommandFactory _commandFactory;
        private readonly CommandRepository _commandRepo;
        private readonly TaskStatusRepository _statusRepo;
        private readonly AgentRepository _agentRepo;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of CommandService with the specified dependencies.
        /// </summary>
        /// <remarks>Dependencies are required for command creation, persistence, and agent/task status
        /// management.</remarks>
        /// <param name="database">Provides database access and persistence operations.</param>
        /// <param name="commandFactory">Factory for creating Command instances.</param>
        /// <param name="commandRepository">Persists and retrieves Command entities.</param>
        /// <param name="taskStatusRepository">Manages persistence and retrieval of task status information.</param>
        /// <param name="agentRepository">Accesses and manages agent data.</param>
        public CommandService(DatabaseService database, CommandFactory commandFactory, CommandRepository commandRepository, TaskStatusRepository taskStatusRepository, AgentRepository agentRepository)
        {
            _commandFactory = commandFactory;
            _commandRepo = commandRepository;
            _statusRepo = taskStatusRepository;
            _agentRepo = agentRepository;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Asynchronously queue a command as a task for the specified agent and persist the created task.
        /// </summary>
        /// <remarks>Assigns the task status to 'Queued', creates the task via the command factory,
        /// enqueues it through the repository, and commits changes.</remarks>
        /// <param name="command">Command to enqueue as a task.</param>
        /// <param name="type">Type of command used to create the task.</param>
        /// <param name="agentID">Identifier of the agent that will own the queued task.</param>
        /// <returns>A Result indicating success, or an error result when the specified agent is not found.</returns>
        public async Task<Result<CommandCreationResponse>> QueueTaskAsync(int agentID, TaskType type, TaskCode code, ICommand command)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);
            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }


            var task = _commandFactory.Create(agentID, type, code, command);
            await _commandRepo.QueueAsync(task);
            await _commandRepo.CommitAsync();

            return FormatCreationResponse(ref type, ref code, task.ID, ref command);
        }

        private CommandCreationResponse FormatCreationResponse(ref TaskType type, ref TaskCode code, int taskID, ref ICommand command)
        {
            string message = type switch
            {

                TaskType.POWERSHELL => $"Tasked agent to run: {((PowerShellDTO)command).Commandlet}",
                TaskType.SHELL => $"Tasked agent to run: {((ShellDTO)command).Command}",
                TaskType.PROCESS when code.Equals(TaskCode.SPAWN_TO) => $"Tasked agent to set spawn to: {((SpawnToDTO)command).Path}",
                TaskType.PROCESS when code.Equals(TaskCode.KILL_PROCESS) => $"Tasked agent to kill process: {((KillProcessDTO)command).PID}",
                TaskType.PROCESS when code.Equals(TaskCode.RUN_AS) => $"Tasked agent to run command as {((RunAsDTO)command).DomainUser}",
                TaskType.PROCESS when code.Equals(TaskCode.RUN) => $"Tasked agent to run program: {((RunDTO)command).Program}",
                TaskType.PROCESS when code.Equals(TaskCode.RUN_U) => $"Tasked agent to run command under process: {((RunUDTO)command).PID}",
                TaskType.DOWNLOAD when code.Equals(TaskCode.DOWNLOAD) => $"Tasked agent to download {((DownloadDTO)command).Path}",
                TaskType.DOWNLOAD when code.Equals(TaskCode.CANCEL_DOWNLOAD) => $"Tasked agent to cancel download with taskID: {((CancelDownloadDTO)command).TaskID}",
                TaskType.UPLOAD => $"Tasked agent to upload file: {((UploadDTO)command).Path}",
                TaskType.SET when code.Equals(TaskCode.UPDATE_HOSTS) => $"Tasked agent to update hosts: {((UpdateHostsDTO)command).Hosts}",
                TaskType.SET when code.Equals(TaskCode.SLEEP) => $"Tasked agent to sleep for {((SetSleepDTO)command).Sleep}s",
                TaskType.GET_UID => $"Tasked agent to get user ID",
                TaskType.KEYLOGGER_START => "Tasked agent to start keylogger",
                TaskType.KEYLOGGER_STOP => "Tasked agent to stop keylogger",
                TaskType.KILL => $"Tasked agent to terminate"
            };

            return new CommandCreationResponse(message, taskID);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//