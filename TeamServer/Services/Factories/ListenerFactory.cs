//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text.Json;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Https;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;
using static Trinity.Shared.Enums.ListenerType;

namespace TeamServer.Services.Factories
{
    public class ListenerFactory : IListenerFactory
    {
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public Listener CreateHttpListener(CreateHttpListenerRequest request)
        {
            return new Listener { Name = request.Name, Type = HTTP, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public Listener CreateHttpsListener(CreateHttpsListenerRequest request)
        {
            return new Listener { Name = request.Name, Type = HTTPS, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public Listener CreateSmbListener(CreateSmbRequest request)
        {
            return new Listener { Name = request.Name, Type = SMB, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        public Listener CreateTcpListener(CreateTcpListenerRequest request)
        {
            return new Listener { Name = request.Name, Type = TCP, Config = JsonDocument.Parse(JsonSerializer.Serialize(request.Config)) };
        }

    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//