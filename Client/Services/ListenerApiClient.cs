using Refit;
using System.Text.Json;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Https;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record ListenerResponse(int id, string name, string protocol);

public sealed record ListenerDetails(
    int Id, string Name, string Protocol,
    int C2Port, int BindPort,
    List<string> Hosts, string RotationStrategy,
    string? UserAgent, string? Header);

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

    public async Task<ListenerDetails?> GetListenerDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await api.GetListenerByIDAsync(id);
            var root = resp.config.RootElement;

            var proto = resp.name.Contains("https", StringComparison.OrdinalIgnoreCase) ? "HTTPS" : "HTTP";

            TryGet(root, out var c2El,  "C2Port",           "c2Port");
            TryGet(root, out var bpEl,  "BindPort",         "bindPort");
            TryGet(root, out var uaEl,  "UserAgent",        "userAgent");
            TryGet(root, out var hdEl,  "Header",           "header");
            TryGet(root, out var rotEl, "RotationStrategy", "rotationStrategy");

            var hosts = new List<string>();
            TryGet(root, out var hostsEl, "Hosts", "hosts");
            if (hostsEl.ValueKind == JsonValueKind.Array)
                foreach (var h in hostsEl.EnumerateArray())
                    hosts.Add(h.GetString() ?? "");

            return new ListenerDetails(
                Id: resp.ID,
                Name: resp.name,
                Protocol: proto,
                C2Port: c2El.ValueKind == JsonValueKind.Number ? c2El.GetInt32() : 443,
                BindPort: bpEl.ValueKind == JsonValueKind.Number ? bpEl.GetInt32() : 8080,
                Hosts: hosts,
                RotationStrategy: rotEl.ValueKind == JsonValueKind.String ? rotEl.GetString()! : "round-robin",
                UserAgent: uaEl.ValueKind == JsonValueKind.String ? uaEl.GetString() : null,
                Header: hdEl.ValueKind == JsonValueKind.String ? hdEl.GetString() : null);
        }
        catch (ApiException) { return null; }
    }

    private static void TryGet(JsonElement root, out JsonElement el, params string[] keys)
    {
        foreach (var k in keys)
            if (root.TryGetProperty(k, out el)) return;
        el = default;
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

    public async Task<bool> CreateHttpListenerAsync(string name, List<string> hosts, int bindPort, int c2Port, string? userAgent, string? header, string rotationStrategy, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateHttpListenerRequest(name, new HttpConfig(bindPort, c2Port, userAgent, header, hosts, rotationStrategy, "3"));
            await api.CreateHttpListenerAsync(request);
            return true;
        }
        catch (ApiException) { return false; }
    }

    public async Task<bool> CreateHttpsListenerAsync(string name, List<string> hosts, int c2Port, string? userAgent, string? header, string rotationStrategy, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CreateHttpsListenerRequest(name, new HttpsConfig(c2Port, userAgent, header, hosts, rotationStrategy, "3"));
            await api.CreateHttpsListenerAsync(request);
            return true;
        }
        catch (ApiException) { return false; }
    }

    public async Task<bool> UpdateHttpAsync(int id, List<string> hosts, int bindPort, int c2Port, string? userAgent, string? header, string rotationStrategy, CancellationToken cancellationToken = default)
    {
        try
        {
            await api.UpdateHttpListenerAsync(id, new UpdateHttpListenerRequest(new HttpConfig(bindPort, c2Port, userAgent, header, hosts, rotationStrategy, "3")));
            return true;
        }
        catch (ApiException) { return false; }
    }

    public async Task<bool> UpdateHttpsAsync(int id, List<string> hosts, int c2Port, string? userAgent, string? header, string rotationStrategy, CancellationToken cancellationToken = default)
    {
        try
        {
            await api.UpdateHttpsListenerAsync(id, new UpdateHttpsListenerRequest(new HttpsConfig(c2Port, userAgent, header, hosts, rotationStrategy, "3")));
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
