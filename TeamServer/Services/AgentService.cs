//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;
using TeamServer.Interface;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.DTOs.Checkin;
using Trinity.Shared.Enums;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;
using static Trinity.Shared.Constants.CheckinRequestOffsets;
using static Trinity.Shared.Constants.SectionLayout;

namespace TeamServer.Services
{
    public class AgentService : IAgentService
    {
        private readonly AgentRepository _agentRepo;
        private readonly AgentFactory _agentFactory;
        private readonly TaskRepository _taskRepo;
        private readonly TaskResultRepository _taskResultRepo;
        private readonly TaskResultFactory _taskResultFactory;
        private readonly HostRepository _hostRepo;
        private readonly PayloadRepository _payloadRepo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ICheckinResponseHandler _responseHandler;
        private readonly IHostFactory _hostFactory;
        private readonly ITaskRequestFactory _taskRequestFactory;
        private readonly IFileTransferService _fileTransferService;


        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public AgentService(AgentRepository agentRepository, TaskRepository taskRepository,
            AgentFactory agentFactory, TaskResultRepository taskResultRepository, TaskResultFactory taskResultFactory,
            HostRepository hostRepository, PayloadRepository payloadRepository, IHubContext<NotificationHub> hubContext,
            ICheckinResponseHandler checkinResponseHandler, IHostFactory hostFactory,
            ITaskRequestFactory taskRequestFactory, IFileTransferService fileTransferService)
        {
            _agentRepo = agentRepository;
            _agentFactory = agentFactory;
            _taskRepo = taskRepository;
            _taskResultFactory = taskResultFactory;
            _taskResultRepo = taskResultRepository;
            _hostRepo = hostRepository;
            _payloadRepo = payloadRepository;
            _hubContext = hubContext;
            _responseHandler = checkinResponseHandler;
            _hostFactory = hostFactory;
            _taskRequestFactory = taskRequestFactory;
            _fileTransferService = fileTransferService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Processes an agent check-in using a base64-encoded blob and returns the result.
        /// </summary>
        /// <param name="blob">A base64-encoded string containing agent check-in data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Result with the formatted
        /// check-in response.</returns>
        public async Task<Result<string>> Checkin(string blob)
        {
            // Decode base64
            var decodedBytes = Util.Base64Decode(blob);
            // Get the UUID
            var blobSpan = new ReadOnlyMemory<byte>(decodedBytes);
            var uuid = Encoding.UTF8.GetString(blobSpan.Slice(RequestStartIndex, UUIDOffset).Span);
            var response = blobSpan.Slice(36, blobSpan.Length - UUIDOffset);

            var agent = await _agentRepo.GetAgentByCheckInUUID(uuid);
            if (agent == null)
            {
                return await ProcessIntialCheckin(uuid, response);
            }

            var request = await ProcessFullyEstablishedCheckin(agent.ID, uuid, response);
            return FormatRequest(uuid, request);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates an Agent session and stores host metadata.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private async Task<string> ProcessIntialCheckin(string uuid, ReadOnlyMemory<byte> requestBody)
        {
            var data = JsonSerializer.Deserialize<IntialCheckInRequest>(requestBody.Span) ?? new IntialCheckInRequest();
            var payload = await _payloadRepo.GetPayloadByUUIDAsync(uuid);

            var agent = _agentFactory.Create(data!, payload!.AES256KEY, 5000, 10, Util.GetEnumValue<Architectures>(payload.Architecture), payload.ID);
            await _agentRepo.CreateAgentAsync(agent);
            await _agentRepo.CommitAsync();

            var host = _hostFactory.CreateHost(agent.ID, ref data!);
            await _hostRepo.AddHostAsync(host);
            await _hostRepo.CommitAsync();

            return CreateBase64Blob(agent.CheckInUUID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stores task responses from the agent and retrieves queued task for dispatch.
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private async Task<string> ProcessFullyEstablishedCheckin(int agentID, string agentKey, ReadOnlyMemory<byte> requestBody)
        {
            using JsonDocument doc = JsonDocument.Parse(requestBody);
            var taskResults = doc.RootElement.EnumerateArray();

            if (taskResults.Any())
            {
                while (taskResults.MoveNext())
                {
                    await _responseHandler.HandleResponseAsync(taskResults.Current);
                    await AdvanceDownloadAsync(taskResults.Current, agentKey);
                }
            }

            var task = await _taskRepo.GetTaskFromQueueAsync(agentID);
            if (task?.CommandType == TaskType.DOWNLOAD && task.Code == TaskCode.DOWNLOAD)
            {
                var command = await _fileTransferService.PrepareDownloadChunkAsync(task, agentKey);
                await _taskRepo.CommitAsync();
                return _taskRequestFactory.Create(task, command);
            }
            return _taskRequestFactory.Create(task);
        }

        private async Task AdvanceDownloadAsync(JsonElement response, string agentKey)
        {
            if (!response.TryGetProperty("Status", out var statusElement)
                || statusElement.GetInt32() != (int)Status.SUCCESS)
                return;

            var taskId = response.GetProperty("TaskID").GetInt32();
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task?.CommandType != TaskType.DOWNLOAD || task.Code != TaskCode.DOWNLOAD)
                return;

            var final = task.Command.RootElement.TryGetProperty("Final", out var finalElement)
                && finalElement.GetBoolean();
            if (!final)
            {
                task.Status = Status.QUEUED;
                await _taskRepo.CommitAsync();
            }
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public async Task<Result<List<AgentResponse>>> GetAgentsAsync()
        {
            return await _agentRepo.GetAgentsAsync();
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        private string CreateBase64Blob(string uuid)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(uuid);
            return System.Convert.ToBase64String(bytes);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        private string FormatRequest(string uuid, string request)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(uuid + request);
            return Convert.ToBase64String(bytes);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//