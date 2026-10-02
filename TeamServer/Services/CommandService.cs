//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using Trinity.Shared.Enums;
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
        public async Task<Result> QueueTaskAsync(ICommand command, CommandTypes type, int agentID)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);
            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }

            var statusID = await _statusRepo.GetTaskStatusID("Queued");
            var task = _commandFactory.Create(agentID, type, command, statusID);
            await _commandRepo.QueueAsync(task);
            await _commandRepo.CommitAsync();

            return Result.Success();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//