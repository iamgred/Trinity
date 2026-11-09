using Refit;
using Trinity.Shared.Models;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public class OperatorService(IOperatorAPI api)
{
    public async Task<List<Operator>> GetTasksAsync()
    {
        try { return await api.GetOperatorsAsync() ?? []; }
        catch (ApiException) { return []; }
    }

    public async Task<Operator?> GetByIdAsync(int id)
    {
        try { return await api.GetByIdAsync(id); }
        catch (ApiException) { return null; }
    }

    public async Task<bool> CreateAsync(string username, string password, string role = "operator")
    {
        try { await api.CreateAsync(new { Username = username, Password = password, Role = role }); return true; }
        catch (ApiException) { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try { await api.DeleteAsync(id); return true; }
        catch (ApiException) { return false; }
    }
}
