//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Trinity.Shared.Enums;

namespace Trinity.Shared.DTOs.Listener
{
    public class HttpListenerDTO
    {
        public int ID { get; set; } = default;
        public string Name { get; set; } = String.Empty;
        public string Type { get; set; } = String.Empty;
        public List<string> Hosts { get; set; } = new(); // Holds hosts for agent communication rotation strategy.
        public int HttpC2BindPort { get; set; } = default;
        public int HttpBindPort { get; set; } = default;
        public string UserAgent { get; set; } = String.Empty;
        public string HttpHostHeader { get; set; } = String.Empty; 
        public string HostRotationStrategy { get; set; } = String.Empty;
        public string MaxRetryStrategy { get; set; } = String.Empty;
        // Number of agents currently connected through this listener (design-16 listener rows).
        public int AgentCount { get; set; } = default;
        //public GuardRailsDTO? GuardRails { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//