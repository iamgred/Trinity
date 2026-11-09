//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Refit;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Https;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.DTOs.Listener.Smb;

namespace Trinity.Shared.Interfaces
{
    public interface IListenerAPI
    {
        [Get("/api/v1/listeners")]
        Task<List<ListenerResponse>> GetListenersAsync();

        [Post("/api/v1/listeners/http")]
        Task CreateHttpListenerAsync([Body] CreateHttpListenerRequest request);

        [Post("/api/v1/listeners/https")]
        Task CreateHttpsListenerAsync([Body] CreateHttpsListenerRequest request);

        [Post("/api/v1/listeners/tcp")]
        Task CreateTcpListenerAsync([Body] CreateTcpListenerRequest request);

        [Post("/api/v1/listeners/smb")]
        Task CreateSmbListenerAsync([Body] CreateSmbRequest request);

        [Delete("/api/v1/listeners/{id}")]
        Task DeleteAsync(int id);

        [Post("/api/v1/listeners/http/restart/{id}")]
        Task RestartAsync(int id);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
