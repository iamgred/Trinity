using Trinity.Shared.DTOs.Command;
using System.Net.Http.Json;

using TrinityTask = Trinity.Shared.Models.Task;
using AgentModel = Trinity.Shared.Models.Agent;     
using CampaignModel = Trinity.Shared.Models.Campaign;  

namespace Client.Services;

public class TaskApiService
{
    private readonly HttpClient _http;

    public TaskApiService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    public async Task<List<TrinityTask>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<TrinityTask>>("/api/v1/tasks") ?? [];

    public async Task<TrinityTask?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<TrinityTask>($"/api/v1/tasks/{id}");

    public async Task<List<AgentModel>> GetAgentsAsync()
        => await _http.GetFromJsonAsync<List<AgentModel>>("/api/v1/agents") ?? [];

    public async Task<List<CampaignModel>> GetCampaignsAsync()
        => await _http.GetFromJsonAsync<List<CampaignModel>>("/api/v1/campaigns") ?? [];

    public async Task<CampaignModel?> CreateCampaignAsync(string name)
    {
        var body = new { Name = name };
        var json = System.Text.Json.JsonSerializer.Serialize(body);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/api/v1/campaigns", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CampaignModel>();
    }

    public async Task<AsyncCommandResponseDTO?> PostCommandAsync(string endpoint, object dto)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(dto, dto.GetType());
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _http.PostAsync(endpoint, content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AsyncCommandResponseDTO>();
    }
}