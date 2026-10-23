using System.Net.Http.Json;
using Trinity.Shared.Models;

namespace Client.Services;

public class ListenerService
{
    private readonly HttpClient _http;

    public ListenerService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    /// <summary>
    /// GET /api/v1/listeners — returns all configured listeners.
    /// </summary>
    public async Task<List<Listener>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Listener>>("/api/v1/listeners") ?? [];

    /// <summary>
    /// GET /api/v1/listeners/{id} — returns a single listener by ID.
    /// </summary>
    public async Task<Listener?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<Listener>($"/api/v1/listeners/{id}");

    /// <summary>
    /// POST /api/v1/listeners — creates a new listener.
    /// Pass the full Listener object; the server assigns the ID.
    /// </summary>
    public async Task<bool> CreateAsync(Listener listener)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/listeners", listener);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// DELETE /api/v1/listeners/{id} — stops and removes a listener.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/listeners/{id}");
        return response.IsSuccessStatusCode;
    }
}