//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using TeamServer.Repositories;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.Results;
using TeamServer.Utils;
using System.Runtime.InteropServices;


namespace TeamServer.Services
{
    public class AgentService
    {
        private readonly AgentRepository _agentRepo;
        private readonly TaskRepository _taskRepo;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public AgentService(AgentRepository agentRepository, TaskRepository taskRepository)
        {
            _agentRepo = agentRepository;
            _taskRepo = taskRepository;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // Checkin 
        public void Checkin(string blob)
        {

            // Decode base64
            var decodedBytes = Util.Base64Decode(blob);
            ProcessIntialCheckin(decodedBytes);
            Span<byte> blobSpan = new Span<byte>(decodedBytes);
            // Get the UUID
            var uuid = System.Text.Encoding.UTF8.GetString(blobSpan.Slice(0, 36));
            var testSize = int.Parse(blobSpan.Slice(36, 1));

            // Get the payload segment
             Span<byte> payloadBytes = blobSpan.Slice(36, blobSpan.Length - 36);
            var response = System.Text.Encoding.Default.GetString(payloadBytes);

            //// Slice the ID 
            //Span<byte> blobSpan = new Span<byte>(decodedBytes);
            //var IDChunk = blobSpan.Slice(0, 1);
            //var ID = int.Parse(IDChunk);
            //var responseSlice = blobSpan.Slice(1, blobSpan.Length - 1);
            //var response = System.Text.Encoding.Default.GetString(responseSlice);
            //// Query the task ID 
        }

        private test ProcessIntialCheckin(byte[] blob) 
        {
            var items = new List<string>();
            int blobLen = blob.Length, baseIndex = 0;
            Span<byte> blobSpan = new Span<byte>(blob);

            // Get the UUID
            baseIndex = blobSpan.Slice(0, 36).Length;
            var uuid = System.Text.Encoding.UTF8.GetString(blobSpan.Slice(0, 36));

            while (baseIndex < blobLen)
            {
                var size = int.Parse(blobSpan.Slice(baseIndex, 2));
                baseIndex = baseIndex + 2;
                var type = int.Parse(blobSpan.Slice(baseIndex, 1));
                baseIndex++;
                var payload = System.Text.Encoding.UTF8.GetString(blobSpan.Slice(baseIndex, size));
                items.Add(payload);
                baseIndex = baseIndex + size;
            }

            return new test { internalIP = items[0], externalIP = items[1], PID = int.Parse(items[2]), ProcessName = items[3] };
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
    public class test 
    {
        public string internalIP { get; set; }
        public string externalIP { get; set; }
        public int PID { get; set; }
        public string ProcessName { get; set; }
    }

}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//