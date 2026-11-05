using System.Net.Http.Json;
using Trinity.Shared.Models;

namespace Client.Services;

public class OperatorService
{
    private readonly HttpClient _http;

    public OperatorService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    public async Task<List<Operator>> GetTasksAsync()
        => await _http.GetFromJsonAsync<List<Operator>>("/api/v1/operators") ?? [];

    public async Task<Operator?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<Operator>($"/api/v1/operators/{id}");

    // Changed: now takes (string username, string password, string role) instead of (Operator op)
    public async Task<bool> CreateAsync(string username, string password, string role = "operator")
    {
        var body = new { Username = username, Password = password, Role = role };
        var response = await _http.PostAsJsonAsync("/api/v1/operators", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/operators/{id}");
        return response.IsSuccessStatusCode;
    }
}