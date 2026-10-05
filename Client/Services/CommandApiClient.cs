using System.Net.Http.Json;

namespace Client.Services;

public sealed class CommandApiClient(HttpClient httpClient, TeamServerConnection connection)
{
    public async Task<bool> QueueCommandAsync(int agentId, string pathSuffix, object body, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before running commands.");
        }

        var requestUri = new Uri(connection.BaseAddress, $"api/v1/commands/{agentId}/{pathSuffix}");
        var response = await httpClient.PostAsJsonAsync(requestUri, body, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
