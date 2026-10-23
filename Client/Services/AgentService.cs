using System.Net.Http.Json;
using Trinity.Shared.Models;

namespace Client.Services;

public class AgentService
{
    private readonly HttpClient _http;

    public AgentService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    /// <summary>
    /// GET /api/v1/agents — returns every agent currently checked in.
    /// Returns an empty list (not null) if the server returns nothing.
    /// </summary>
    public async Task<List<Agent>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Agent>>("/api/v1/agents") ?? [];

    /// <summary>
    /// GET /api/v1/agents/{id} — returns one specific agent, or null if not found.
    /// </summary>
    public async Task<Agent?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<Agent>($"/api/v1/agents/{id}");

    /// <summary>
    /// DELETE /api/v1/agents/{id} — removes an agent. Returns true if successful.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/agents/{id}");
        return response.IsSuccessStatusCode;
    }
}