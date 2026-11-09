using Refit;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed class AgentApiClient(IAgentAPI api)
{
    public async Task<IReadOnlyList<AgentResponse>> GetAgentsAsync(CancellationToken cancellationToken = default)
    {
        try { return await api.GetAgentsAsync() ?? []; }
        catch (ApiException ex) { throw new HttpRequestException(ex.Message, ex); }
    }
}
