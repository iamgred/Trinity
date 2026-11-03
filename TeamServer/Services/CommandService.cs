﻿//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
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
        private readonly ToolRepository _toolRepo;
        private readonly IWebHostEnvironment _env;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="commandFactory"></param>
        /// <param name="commandRepository"></param>
        /// <param name="taskStatusRepository"></param>
        /// <param name="agentRepository"></param>
        /// <param name="toolRepository"></param>
        public CommandService(DatabaseService database,
            CommandFactory commandFactory, CommandRepository commandRepository,
            TaskStatusRepository taskStatusRepository, AgentRepository agentRepository,
            ToolRepository toolRepository, IWebHostEnvironment env)
        {
            _commandFactory = commandFactory;
            _commandRepo = commandRepository;
            _statusRepo = taskStatusRepository;
            _agentRepo = agentRepository;
            _toolRepo = toolRepository;
            _env = env;
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
        public async Task<Result> QueueTaskAsync(int agentID, TaskType type, TaskCode code, ICommand command)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);
            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }

            var task = _commandFactory.Create(agentID, type, code, command);
            await _commandRepo.QueueAsync(task);
            await _commandRepo.CommitAsync();
            return Result.Success();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Result> QueueNetAsmTask(int agentID, TaskType type, TaskCode code, DotNetAsmDTO command)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);
            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }

            var tool = await _toolRepo.GetToolByPathAsync(command.AssemblyPath);
            if (tool == null)
            {
                return ToolError.NotFound(command.AssemblyPath);
            }

            var blob = await ConvertToBase64Blob(command.AssemblyPath);
            var request = new DotNetAsmRequest { Arguments = command.Arguments, Assembly = blob };

            var task = _commandFactory.Create(agentID, type, code, request);
            await _commandRepo.QueueAsync(task);
            await _commandRepo.CommitAsync();
            return Result.Success();
        }

        private async Task<string> ConvertToBase64Blob(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                byte[] fileBytes = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileBytes, 0, fileBytes.Length);
                string base64blob = Convert.ToBase64String(fileBytes);
                return base64blob;
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//