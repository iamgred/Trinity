using System.Net.Http.Json;
using Trinity.Shared.DTOs.Payload;

namespace Client.Services;

/* what the Armory table binds to in Payloads.razor */
public sealed record PayloadEntry(
    string Name,
    long Size,
    string Type,
    DateTime CreatedAt);

public class PayloadService
{
    private readonly HttpClient _http;

    public PayloadService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    public async Task<List<PayloadEntry>> GetTasksAsync()
    {
        var response = await _http.GetAsync("/api/v1/payloads");

        /* 400 means the payload table is empty — not a real error */
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            return [];

        response.EnsureSuccessStatusCode();

        var dtos = await response.Content.ReadFromJsonAsync<List<PayloadDTO>>() ?? [];
        return dtos.Select(d => new PayloadEntry(d.Name, d.Size, d.Type, d.CreateTime)).ToList();
    }

    /* renamed from CreateAsync — hits the correct /generate endpoint */
    public async Task<bool> GenerateAsync(PayloadCreationDTO dto)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/payloads/generate", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/payloads/{id}");
        return response.IsSuccessStatusCode;
    }

    /* build the download URL for a payload by its ID */
    public string GetDownloadUrl(int payloadId) => $"/api/v1/payloads/{payloadId}";
}