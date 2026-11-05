using System.Net.Http.Json;
using Trinity.Shared.Models;

namespace Client.Services;

public class CampaignService
{
    private readonly HttpClient _http;

    public CampaignService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    public async Task<List<Campaign>> GetTasksAsync()
        => await _http.GetFromJsonAsync<List<Campaign>>("/api/v1/campaigns") ?? [];

    public async Task<Campaign?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<Campaign>($"/api/v1/campaigns/{id}");

    // Changed: now takes (string name, string description) instead of (Campaign campaign)
    public async Task<bool> CreateAsync(string name, string description = "")
    {
        var body = new { Name = name, Description = description };
        var response = await _http.PostAsJsonAsync("/api/v1/campaigns", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/campaigns/{id}");
        return response.IsSuccessStatusCode;
    }
}