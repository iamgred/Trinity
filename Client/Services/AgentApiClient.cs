using System.Net.Http.Json;
using System.Text.Json;
using Trinity.Shared.DTOs.Agent;

namespace Client.Services;

public sealed class AgentApiClient(HttpClient httpClient, TeamServerConnection connection)
{
    public async Task<IReadOnlyList<AgentResponse>> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before viewing clients.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/agents");
        var agents = await httpClient.GetFromJsonAsync<List<AgentResponse>>(requestUri, cancellationToken);
        return agents ?? throw new JsonException("The TeamServer returned an empty agent list response.");
    }
}
