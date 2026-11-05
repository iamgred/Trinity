// ============================================== { START OF FILE} ============================================== //
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Https;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IListenerService
    {
        Task<Result<List<ListenerResponse>>> GetListenersAsync();
        Task<Result> RestartListenerAsync(int ID);
        Task<Result> CreateHttpListenerAsync(CreateHttpListenerRequest request);
        Task<Result> CreateHttpsListenerAsync(CreateHttpsListenerRequest request);
        Task<Result> CreateTCPListenerAsync(CreateTcpListenerRequest request);
        Task<Result> CreateSmbListenerAsync(CreateSmbRequest request);
        Task<Result<GetListenerDetailsResponse>> GetListener(int ID);
        Task<Result> UpdateHttpListenerAsync(int ID, HttpConfig config);
        Task<Result> UpdateHttpsListenerAsync(int ID, HttpsConfig config);
        Task<Result> RemoveListenerAsync(int ID);
    }
}
// ============================================== { END OF FILE} ============================================== //