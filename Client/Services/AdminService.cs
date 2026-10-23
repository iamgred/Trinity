using System.Net.Http.Json;
using Trinity.Shared.Models;

namespace Client.Services;

public class AdminService
{
    private readonly HttpClient _http;

    public AdminService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    // <summary>
    // GET /api/v1/admins — returns all admin accounts.
    // </summary>
    public async Task<List<Admin>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<Admin>>("/api/v1/admins") ?? [];

    // <summary>
    // GET /api/v1/admins/{id} — returns a single admin.
    // </summary>
    public async Task<Admin?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<Admin>($"/api/v1/admins/{id}");

    // <summary>
    // POST /api/v1/admins — creates a new admin account.
    // </summary>
    public async Task<bool> CreateAsync(Admin admin)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/admins", admin);
        return response.IsSuccessStatusCode;
    }

    // <summary>
    // DELETE /api/v1/admins/{id} — removes an admin.
    // </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"/api/v1/admins/{id}");
        return response.IsSuccessStatusCode;
    }
}

