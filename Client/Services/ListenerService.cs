using Refit;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.Interfaces;
using SharedListenerResponse = Trinity.Shared.DTOs.Listener.ListenerResponse;

namespace Client.Services;

public class ListenerService(IListenerAPI api)
{
    public async Task<List<SharedListenerResponse>> GetTasksAsync()
    {
        try { return await api.GetListenersAsync() ?? []; }
        catch (ApiException) { return []; }
    }

    public async Task<bool> CreateHttpAsync(CreateHttpListenerRequest request)
    {
        try { await api.CreateHttpListenerAsync(request); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> CreateTcpAsync(CreateTcpListenerRequest request)
    {
        try { await api.CreateTcpListenerAsync(request); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> CreateSmbAsync(CreateSmbRequest request)
    {
        try { await api.CreateSmbListenerAsync(request); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }
}
