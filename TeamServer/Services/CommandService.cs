//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using TeamServer.Interface;
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class CommandService : ICommandService
    {
        private readonly CommandFactory _commandFactory;
        private readonly CommandRepository _commandRepo;
        private readonly TaskStatusRepository _statusRepo;
        private readonly AgentRepository _agentRepo;
        private readonly ToolRepository _toolRepo;
        private readonly IWebHostEnvironment _env;
        private readonly IDownloadService _downloadService;

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
        public CommandService(
            CommandFactory commandFactory, CommandRepository commandRepository,
            TaskStatusRepository taskStatusRepository, AgentRepository agentRepository,
            ToolRepository toolRepository, IWebHostEnvironment env, IDownloadService downloadService)
        {
            _commandFactory = commandFactory;
            _commandRepo = commandRepository;
            _statusRepo = taskStatusRepository;
            _agentRepo = agentRepository;
            _toolRepo = toolRepository;
            _env = env;
            _downloadService = downloadService;
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
        /// <summary>
        /// Queues a download task for the specified agent and command, creating the first chunk of the download.
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<Result> QueueDownloadTaskAsync(int agentID, DownloadDTO command)
        {
            var agent = await _agentRepo.GetAgentByIDAsync(agentID);

            if (agent == null)
            {
                return AgentError.NotFound(agentID);
            }

            var firstChunkResult =
                await _downloadService.DownloadChunkAsync(
                    command.Path,
                    command.DestinationPath,
                    0);

            if (!firstChunkResult.IsSuccess)
            {
                return firstChunkResult.Error;
            }

            var firstChunk = firstChunkResult.Response;

            if (firstChunk == null)
            {
                return Error.Failure(
                    "Download.FirstChunkMissing",
                    "The first download chunk could not be created.");
            }

            var task = _commandFactory.Create(
                agentID,
                TaskType.DOWNLOAD,
                firstChunk.Final
                    ? TaskCode.DOWNLOAD_FINAL
                    : TaskCode.DOWNLOAD_CHUNK,
                firstChunk);

            await _commandRepo.QueueAsync(task);
            await _commandRepo.CommitAsync();

            return Result.Success();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//