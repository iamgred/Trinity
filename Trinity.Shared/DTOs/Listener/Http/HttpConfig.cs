//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using Trinity.Shared.Interfaces;

namespace Trinity.Shared.DTOs.Listener.Http
{
    public record HttpConfig : IListenerConfig
    {
        public int BindPort { get; init; }
        public int C2Port { get; init; }
        public string UserAgent { get; init; }
        public string Header { get; init; }
        public List<string> Hosts { get; init; }
        public string RotationStrategy { get; init; }
        public string MaxRetryStrategy { get; init; }

        public HttpConfig(int bindPort, int c2Port, string? userAgent, string? header, List<string> hosts, string rotationStrategy, string maxRetryStrategy)
        {
            BindPort = bindPort;
            C2Port = c2Port;
            UserAgent = userAgent ?? string.Empty;
            Header = header ?? "Content-type: */*";
            RotationStrategy = rotationStrategy;
            MaxRetryStrategy = maxRetryStrategy;
            if (hosts.Count <= 0)
            {
                throw new ArgumentException("A host must be specified!");
            }
            Hosts = hosts;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//