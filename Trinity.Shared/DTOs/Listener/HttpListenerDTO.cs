//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using Trinity.Shared.Enums;

namespace Trinity.Shared.DTOs.Listener
{
    public class HttpListenerDTO
    {
        public string ID { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public ListenerTypes Type { get; set; } = ListenerTypes.HTTP;
        public List<string> Hosts { get; set; } = new(); // Holds hosts for agent communication rotation strategy.
        public int HttpC2BindPort { get; set; } = default;
        public int HttpBindPort { get; set; } = default;
        public string UserAgent { get; set; } = String.Empty;
        public string HttpHostHeader { get; set; } = String.Empty; 
        public string HostRotationStrategy { get; set; } = String.Empty;
        public string MaxRetryStrategy { get; set; } = String.Empty;
        public GuardRailsDTO? GuardRails { get; set; }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//