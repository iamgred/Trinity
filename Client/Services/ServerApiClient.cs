using Refit;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record ServerIpResponse(string ipV4, string ipV6);

public sealed class ServerApiClient(IServerAPI api)
{
    public async Task<ServerIpResponse?> GetServerIpAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var r = await api.GetServerIpAsync();
            return new ServerIpResponse(r.ipV4, r.ipV6);
        }
        catch (ApiException) { return null; }
    }

    public async Task<bool> GetServerStatusAsync(CancellationToken cancellationToken = default)
    {
        try { await api.GetStatusAsync(); return true; }
        catch (ApiException) { return false; }
    }
}
