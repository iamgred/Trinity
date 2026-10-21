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


        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public AgentService(AgentRepository agentRepository, TaskRepository taskRepository, AgentFactory agentFactory, TaskResultRepository taskResultRepository, TaskResultFactory taskResultFactory, HostRepository hostRepository, PayloadRepository payloadRepository, IHubContext<NotificationHub> hubContext)
        {
            _agentRepo = agentRepository;
            _agentFactory = agentFactory;
            _taskRepo = taskRepository;
            _taskResultFactory = taskResultFactory;
            _taskResultRepo = taskResultRepository;
            _hostRepo = hostRepository;
            _payloadRepo = payloadRepository;
            _hubContext = hubContext;
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
            var request = blobSpan.Slice(36, blobSpan.Length - UUIDOffset);

            var agent = await _agentRepo.GetAgentByCheckInUUID(uuid);
            if (agent == null)
            {
                return await ProcessIntialCheckin(uuid, request);
            }


        Get_Task:
            var task = await _taskRepo.GetTaskFromQueueAsync(agent.ID);

            if (task == null)
            {
                return string.Empty;
            }
            return CreateResponseBlob(agent.CheckInUUID, new CheckinResponse(task.ID, (int)task.CommandType, task.Command, task.CreatedAt));
        }

        private string CreateBase64Blob(string uuid)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(uuid);
            return System.Convert.ToBase64String(bytes);
        }

        private string CreateResponseBlob(string uuid, CheckinResponse response)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var jsonWriter = new Utf8JsonWriter(bufferWriter))
            {
                JsonSerializer.Serialize(jsonWriter, response);
            }

            return System.Convert.ToBase64String(bufferWriter.WrittenSpan);
        }

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

            return CreateBase64Blob(agent.CheckInUUID); ;
        }

        private async Task<string> ProcessFullyEstablishedCheckin(Agent agent, ReadOnlyMemory<byte> requestBody)
        {
            using JsonDocument doc = JsonDocument.Parse(requestBody);
            var taskResults = doc.RootElement.EnumerateArray();

            // Has task results 
            if (taskResults.Count() > 0)
            {
                while (taskResults.MoveNext())
                {

                }
            }

            // Get tasking 

            return ""; // Return the blob with tasking 
        }

        private async System.Threading.Tasks.Task ProcessTaskResponse(JsonElement element)
        {
            var status = (Status) element.GetProperty("status").GetUInt32();
            var taskID = element.GetProperty("taskID").GetInt32();
            var task = await _taskRepo.GetByIdAsync(taskID) ?? throw new NullReferenceException();

            switch (status)
            {
                case Status.ERROR:
                    var error = JsonSerializer.Deserialize<TaskResp<Error>>(element);
                    // Insert error into DB 
                    // Update the status of the task record
                    break;
                case Status.SUCCESS:
                {
                    var response = JsonSerializer.Deserialize<TaskResp<string>>(element);
                    task.StatusID = (int)status;
                    task.Result = new TaskResult { TaskID = task.ID, Response = response.Data };
                    await _taskRepo.CommitAsync();
                    break;
                }
                case Status.RUNNING:
                    task.StatusID = (int)status;
                    await _taskRepo.CommitAsync();
                    break;
                case Status.MORE_TO_FOLLOW:
                    break;
                case Status.NO_MORE_TO_FOLLOW:
                    break;
                default:
                    throw new ArgumentException("Invalid status provided in task header!");
            }   
        }


        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetAgentInfo

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetAgents => filters : campaign
        public async Task<Result<List<AgentResponse>>> GetAgentsAsync()
        {
            return await _agentRepo.GetAgentsAsync();
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//