using System.Net.Http.Json;

namespace Client.Services;

public sealed record ServerIpResponse(string ipV4, string ipV6);

public sealed class ServerApiClient(HttpClient httpClient, TeamServerConnection connection)
{
    public async Task<ServerIpResponse?> GetServerIpAsync(CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before viewing server info.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/server/teamserverip");
        return await httpClient.GetFromJsonAsync<ServerIpResponse>(requestUri, cancellationToken);
    }
}
