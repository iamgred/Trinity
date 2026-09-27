//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Trinity.Shared.DTOs.Listener
{
    public record CreateHttpListenerRequest
    {
        public string Name { get; init; }
        public int BindPort { get; init; }
        public int C2Port { get; init; }
        public string UserAgent { get; init; }
        public string Header { get; init; }
        public List<string> Hosts { get; init; }
        public string RotationStrategy { get; init; } 
        public string MaxRetryStrategy { get; init; }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Initializes a new instance of CreateHttpListenerRequest with the specified settings.
        /// </summary>
        /// <remarks>If userAgent or header are null they default to an empty string and "Content-type:
        /// */*" respectively; hosts must contain at least one element.</remarks>
        /// <param name="name">Listener name.</param>
        /// <param name="bindPort">TCP port to bind the HTTP listener to.</param>
        /// <param name="c2Port">Port used for C2 (command-and-control) communication.</param>
        /// <param name="userAgent">Optional User-Agent header value; defaults to an empty string when null.</param>
        /// <param name="header">Optional HTTP header to include; defaults to "Content-type: */*" when null.</param>
        /// <param name="hosts">Collection of hostnames; must contain at least one entry.</param>
        /// <param name="rotation">Rotation strategy identifier.</param>
        /// <param name="maxRetry">Maximum-retry strategy identifier or value.</param>
        /// <exception cref="ArgumentException">Thrown if hosts contains no entries.</exception>
        public CreateHttpListenerRequest(string name, int bindPort, int c2Port, string? userAgent, string? header, List<string> hosts, string rotationStrategy, string maxRetryStrategy)
        {
            Name = name;
            BindPort = bindPort;
            C2Port = c2Port;
            UserAgent = userAgent ?? string.Empty;
            Header = header ?? "Content-type: */*";
            RotationStrategy = rotationStrategy;
            MaxRetryStrategy = maxRetryStrategy;
            Hosts = hosts;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//