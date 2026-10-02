//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System;
using System.Collections.Generic;
using System.Text;
using Trinity.Shared.Models;
using Trinity.Shared.DTOs;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.DTOs.Listener.Smb;

namespace Trinity.Shared.Interfaces
{
    public interface IListenerFactory
    {
        public Listener CreateHttpListener(CreateHttpListenerRequest request, int protocolID);
        public Listener CreateTcpListener(CreateTcpListenerRequest request, int protocolID);
        public Listener CreateSmbListener(CreateSmbRequest request, int protocolID);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//