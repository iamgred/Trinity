//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.SignalR;
using System;
using System.Buffers;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TeamServer.NotificationDispatchers;
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.DTOs.Checkin;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;
using Trinity.Shared.Results;
using Trinity.Shared.Enums.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Trinity.Shared.Constants.CheckinRequestOffsets;
using static Trinity.Shared.Constants.SectionLayout;
using TeamServer.Handlers.CheckinHandler;
using Trinity.Shared.Interfaces;



namespace TeamServer.Services
{
    public class AgentService
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


        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public AgentService(AgentRepository agentRepository, TaskRepository taskRepository, 
            AgentFactory agentFactory, TaskResultRepository taskResultRepository, TaskResultFactory taskResultFactory, 
            HostRepository hostRepository, PayloadRepository payloadRepository, IHubContext<NotificationHub> hubContext,
            ICheckinResponseHandler checkinResponseHandler)
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
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // Checkin 
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

            var request = await ProcessFullyEstablishedCheckin(agent.ID, response);
            return FormatRequest(uuid, request);
        }

        private string CreateBase64Blob(string uuid)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(uuid);
            return System.Convert.ToBase64String(bytes);
        }

        private string FormatRequest(string uuid, string request)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(uuid + request);
            return Convert.ToBase64String(bytes);
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
            var data = JsonSerializer.Deserialize<IntialCheckInRequest>(requestBody.Span);
            var payload = await _payloadRepo.GetPayloadByUUIDAsync(uuid);

            var agent = _agentFactory.Create(data!, payload!.AES256KEY, 5000, 10, Util.GetEnumValue<Architectures>(payload.Architecture), payload.ID);
            await _agentRepo.CreateAgentAsync(agent);
            await _agentRepo.CommitAsync();

            var host = new Trinity.Shared.Models.Host
            {
                AgentID = agent.ID,
                CPUCount = data!.CPUCount,
                DiskSize = data.DiskSize,
                FreeDisk = data.FreeDisk,
                OS = data.OS,
                HostName = data.user,
                MACAddress = data.macAddress,
                Motherboard = data.motherboard,
                RAM = data.RAM
            };

            await _hostRepo.AddHostAsync(host);
            await _hostRepo.CommitAsync();

            return CreateBase64Blob(agent.CheckInUUID);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Stores task responses from the agent and retrieves queued tasks for dispatch,
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        private async Task<string> ProcessFullyEstablishedCheckin(int agentID, ReadOnlyMemory<byte> requestBody)
        {
            using JsonDocument doc = JsonDocument.Parse(requestBody);
            var taskResults = doc.RootElement.EnumerateArray();

            if (taskResults.Any())
            {
                while (taskResults.MoveNext())
                {
                    await _responseHandler.HandleResponseAsync(taskResults.Current);
                }
            }

            var task = await _taskRepo.GetTaskFromQueueAsync(agentID);
            var request = String.Empty;

            if (task != null)
            {
                request = JsonSerializer.Serialize(new TaskRequest { TaskID = task.ID, TaskType = (int) task.CommandType, Command = task.Command, Status = (int) Status.DISPATCHED });

            }

            return request;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetAgents => filters : campaign
        public async Task<Result<List<AgentResponse>>> GetAgentsAsync()
        {
            return await _agentRepo.GetAgentsAsync();
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//