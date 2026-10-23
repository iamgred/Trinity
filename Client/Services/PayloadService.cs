using System.Net.Http.Json;
using Trinity.Shared.DTOs.Payload;

namespace Client.Services;

public class PayloadService
{
    private readonly HttpClient _http;

    public PayloadService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    /// <summary>
    /// GET /api/v1/payloads — returns a summary list of all payloads.
    /// Uses PayloadDTO (Name, Size, Type, CreateTime) rather than the full model,
    /// because that's what the API returns for list views.
    /// </summary>
    public async Task<List<PayloadDTO>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<PayloadDTO>>("/api/v1/payloads") ?? [];

    /// <summary>
    /// POST /api/v1/payloads — requests the server to generate a new payload.
    /// PayloadCreationDTO carries the config: listener, campaign, architecture, type, etc.
    /// </summary>
    public async Task<bool> CreateAsync(PayloadCreationDTO dto)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/payloads", dto);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// DELETE /api/v1/payloads/{id} — removes a payload.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/payloads/{id}");
        return response.IsSuccessStatusCode;
    }
}