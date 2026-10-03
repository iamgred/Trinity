//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Net;
using System.Text.Json;
using TeamServer.Exceptions;
using TeamServer.Modules;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.DTOs.Listener.Tor;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace TeamServer.Services.Factories
{
    public class ListenerFactory : IListenerFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a Listener populated with the specified name, protocol identifier, and serialized configuration.
        /// </summary>
        /// <remarks>request.Config is serialized using System.Text.Json and parsed into a JsonDocument;
        /// serialization or parsing may throw exceptions such as JsonException.</remarks>
        /// <param name="request">Request object containing the listener Name and Config used to initialize the Listener.</param>
        /// <param name="protocolID">Protocol identifier to assign to the Listener.</param>
        /// <returns>A Listener with Name and ProtocolID set and Config populated as a JsonDocument serialized from
        /// request.Config.</returns>
        public Listener CreateHttpListener(CreateHttpListenerRequest request, int protocolID)
        {
            return new Listener { Name = request.Name, ProtocolID = protocolID, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a Listener using the request values and the specified protocol identifier.
        /// </summary>
        /// <param name="request">CreateSmbRequest containing the Name and configuration used to populate the Listener.</param>
        /// <param name="protocolID">Protocol identifier to assign to the created Listener.</param>
        /// <returns>A Listener with Name taken from request.Name, ProtocolID set to protocolID, and Config set to a JsonDocument
        /// produced by serializing and parsing request.Config.</returns>
        public Listener CreateSmbListener(CreateSmbRequest request, int protocolID)
        {
            return new Listener { Name = request.Name, ProtocolID = protocolID, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Creates a Listener whose Name is taken from request.Name, ProtocolID is set to protocolID, and Config is
        /// populated by serializing request.Config to JSON and parsing it into a JsonDocument.
        /// </summary>
        /// <remarks>Serializes request.Config using System.Text.Json.JsonSerializer and parses the result
        /// into a JsonDocument; may throw exceptions for null or non-serializable input.</remarks>
        /// <param name="request">Request containing the listener Name and configuration object to apply.</param>
        /// <param name="protocolID">Protocol identifier to assign to the created Listener.</param>
        /// <returns>A new Listener with Name set to request.Name, ProtocolID set to protocolID, and Config containing the
        /// serialized request.Config as a JsonDocument.</returns>
        public Listener CreateTcpListener(CreateTcpListenerRequest request, int protocolID)
        {
            return new Listener { Name = request.Name, ProtocolID = protocolID, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }
        /// <summary>
        /// Creates a Listener whose Name is taken from request.Name, ProtocolID is set to protocolID, and Config is
        /// populated by serializing request.Config to JSON and parsing it into a JsonDocument.
        /// </summary>
        /// <param name="request">Request containing the listener Name and configuration object to apply.</param>
        /// <param name="protocolID">Protocol identifier to assign to the created Listener.</param>
        /// <returns>A new Listener with Name set to request.Name, ProtocolID set to protocolID, and Config containing the
        /// serialized request.Config as a JsonDocument.</returns>
        public Listener CreateTorListener(CreateTorListenerRequest request, int protocolID)
        {
            return new Listener
            {
                Name = request.Name,
                ProtocolID = protocolID,
                Config = JsonDocument.Parse(
                    JsonSerializer.Serialize(request.Config))
            };
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//