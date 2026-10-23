using System.Net.Http.Json;

namespace Client.Services;

public sealed record ListenerResponse(int id, string name, string protocol);

public sealed class ListenerApiClient(HttpClient httpClient, TeamServerConnection connection)
{
    public async Task<IReadOnlyList<ListenerResponse>> GetListenersAsync(CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before viewing listeners.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/listeners");
        var listeners = await httpClient.GetFromJsonAsync<List<ListenerResponse>>(requestUri, cancellationToken);
        return listeners ?? new List<ListenerResponse>();
    }

    public async Task<bool> CreateHttpListenerAsync(string name, string host, int port, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before creating a profile.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/listeners/http");
        var body = new
        {
            Name = name,
            Config = new
            {
                BindPort = port,
                C2Port = port,
                UserAgent = "",
                Header = "Content-type: */*",
                Hosts = new[] { host },
                RotationStrategy = "round-robin",
                MaxRetryStrategy = "3",
            },
        };
        var response = await httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateTcpListenerAsync(string name, int port, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before creating a profile.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/listeners/tcp");
        var body = new { Name = name, Config = new { Port = port } };
        var response = await httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateSmbListenerAsync(string name, string pipe, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before creating a profile.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/listeners/smb");
        var body = new { Name = name, Config = new { pipe } };
        var response = await httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteListenerAsync(int id, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before deleting a profile.");
        }

        var requestUri = new Uri(connection.BaseAddress, $"api/v1/listeners/{id}");
        var response = await httpClient.DeleteAsync(requestUri, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RestartListenerAsync(int id, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before restarting a profile.");
        }

        var requestUri = new Uri(connection.BaseAddress, $"api/v1/listeners/http/restart/{id}");
        var response = await httpClient.PostAsync(requestUri, content: null, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
