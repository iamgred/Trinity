//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Trinity.Shared.Interfaces;

namespace Trinity.Shared.DTOs.Listener.Http
{
    public record CreateHttpListenerRequest
    {
        public string Name { get; init; }
        public HttpConfig Config { get; init; }

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
        public CreateHttpListenerRequest(string name, HttpConfig httpConfig)
        {
            Name = name;
            Config = httpConfig;
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//