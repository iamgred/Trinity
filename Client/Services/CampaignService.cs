using System.Net.Http.Json;
using Trinity.Shared.Models;

namespace Client.Services;

public class CampaignService
{
    private readonly HttpClient _http;

    public CampaignService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    /// <summary>
    /// GET /api/v1/campaigns — returns all campaigns.
    /// </summary>
    public async Task<List<Campaign>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Campaign>>("/api/v1/campaigns") ?? [];

    /// <summary>
    /// GET /api/v1/campaigns/{id} — returns one campaign.
    /// </summary>
    public async Task<Campaign?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<Campaign>($"/api/v1/campaigns/{id}");

    /// <summary>
    /// POST /api/v1/campaigns — creates a new campaign.
    /// </summary>
    public async Task<bool> CreateAsync(Campaign campaign)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/campaigns", campaign);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// DELETE /api/v1/campaigns/{id} — deletes a campaign.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/campaigns/{id}");
        return response.IsSuccessStatusCode;
    }
}