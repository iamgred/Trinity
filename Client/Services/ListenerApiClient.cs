using Refit;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record ListenerResponse(int id, string name, string protocol);

public sealed class ListenerApiClient(IListenerAPI api)
{
    public async Task<IReadOnlyList<ListenerResponse>> GetListenersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await api.GetListenersAsync() ?? [];
            return items.Select(l => new ListenerResponse(l.ID, l.name, l.protocol)).ToList();
        }
        catch (ApiException) { return []; }
    }

    public async Task<bool> CreateHttpListenerAsync(string name, string host, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateHttpListenerRequest(name, new HttpConfig(port, port, null, null, [host], "round-robin", "3"));
            await api.CreateHttpListenerAsync(request);
            return true;
        }
        catch (ApiException) { return false; }
    }

    public async Task<bool> CreateTcpListenerAsync(string name, int port, CancellationToken cancellationToken = default)
    {
        try
        {
            await api.CreateTcpListenerAsync(new CreateTcpListenerRequest(name, new TcpConfig(port)));
            return true;
        }
        catch (ApiException) { return false; }
    }

    public async Task<bool> CreateSmbListenerAsync(string name, string pipe, CancellationToken cancellationToken = default)
    {
        try
        {
            await api.CreateSmbListenerAsync(new CreateSmbRequest(name, new SmbConfig(pipe)));
            return true;
        }
        catch (ApiException) { return false; }
    }

    public async Task<bool> DeleteListenerAsync(int id, CancellationToken cancellationToken = default)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> RestartListenerAsync(int id, CancellationToken cancellationToken = default)
    {
        try { await api.RestartAsync(id); return true; }
        catch (ApiException) { return false; }
    }
}
