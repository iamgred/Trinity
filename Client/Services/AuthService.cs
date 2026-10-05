using System.Net.Http.Json;
using Trinity.Shared.DTOs.Authentication;

namespace Client.Services;

public class AuthService
{
    private readonly HttpClient _http;

    // IHttpClientFactory is injected automatically by .NET.
    // We ask it for the "TeamServer" client we registered in Program.cs.
    public AuthService(IHttpClientFactory factory)
        => _http = factory.CreateClient("TeamServer");

    /// <summary>
    /// Sends the username and password to POST /api/v1/auth/login.
    /// Returns true if the server responds with 200 OK, false otherwise.
    /// Note: Auth is a stub right now — no JWT token is returned yet.
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        var dto = new LoginDTO { Username = username, Password = password };
        var response = await _http.PostAsJsonAsync("/api/v1/auth/login", dto);
        return response.IsSuccessStatusCode; // true = 200, false = 400/401/etc.
    }
}