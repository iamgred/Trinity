using Refit;
using Trinity.Shared.Models;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public class AdminService(IAdminAPI api)
{
    public async Task<List<Admin>> GetTasksAsync()
    {
        try { return await api.GetAdminsAsync() ?? []; }
        catch (ApiException) { return []; }
    }

    public async Task<Admin?> GetByIdAsync(int id)
    {
        try { return await api.GetByIdAsync(id); }
        catch (ApiException) { return null; }
    }

    public async Task<bool> CreateAsync(Admin admin)
    {
        try { await api.CreateAsync(admin); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }
}
