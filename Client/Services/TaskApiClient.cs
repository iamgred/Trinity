using Refit;
using Trinity.Shared.Interfaces;

using TrinityTask = Trinity.Shared.Models.Task;
using AgentModel = Trinity.Shared.Models.Agent;
using CampaignModel = Trinity.Shared.Models.Campaign;

namespace Client.Services;

public class TaskApiService(ITaskAPI taskApi, ICampaignAPI campaignApi)
{
    public async Task<List<TrinityTask>> GetTasksAsync()
    {
        try { return await taskApi.GetTasksAsync() ?? []; }
        catch (ApiException) { return []; }
    }

    public Task<List<TrinityTask>> GetAllTasksAsync() => GetTasksAsync();

    public async Task<TrinityTask?> GetByIdAsync(int id)
    {
        try { return await taskApi.GetByIdAsync(id); }
        catch (ApiException) { return null; }
    }

    public async Task<List<CampaignModel>> GetCampaignsAsync()
    {
        try { return await campaignApi.GetCampaignsAsync() ?? []; }
        catch (ApiException) { return []; }
    }
}
