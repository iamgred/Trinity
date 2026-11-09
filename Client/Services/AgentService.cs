using Refit;
using Trinity.Shared.DTOs.Agent;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public class AgentService(IAgentAPI api)
{
    public async Task<List<AgentResponse>> GetTasksAsync()
    {
        try { return await api.GetAgentsAsync() ?? []; }
        catch (ApiException) { return []; }
    }

    public async Task<AgentResponse?> GetByIdAsync(int id)
    {
        try { return await api.GetByIdAsync(id); }
        catch (ApiException) { return null; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }
}
