using Refit;
using Trinity.Shared.Models;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public class CampaignService(ICampaignAPI api)
{
    public async Task<List<Campaign>> GetTasksAsync()
    {
        try { return await api.GetCampaignsAsync() ?? []; }
        catch (ApiException) { return []; }
    }

    public async Task<Campaign?> GetByIdAsync(int id)
    {
        try { return await api.GetByIdAsync(id); }
        catch (ApiException) { return null; }
    }

    public async Task<bool> CreateAsync(string name, string description = "")
    {
        try { await api.CreateAsync(new { Name = name, Description = description }); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }
}
