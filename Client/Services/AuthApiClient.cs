using System.Net;
using System.Net.Http.Json;
using Trinity.Shared.DTOs.Authentication;

namespace Client.Services;

// Calls the TeamServer auth endpoint; reports success/failure only (no token/session).
public sealed class AuthApiClient(HttpClient httpClient, TeamServerConnection connection)
{
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
    }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (connection.BaseAddress is null)
        {
            throw new InvalidOperationException("Connect to a TeamServer from the Login page before signing in.");
        }

        var requestUri = new Uri(connection.BaseAddress, "api/v1/auth/login");
        var credentials = new LoginDTO { Username = username, Password = password };

        var response = await httpClient.PostAsJsonAsync(requestUri, credentials, cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return LoginResult.InvalidCredentials;
        }

        response.EnsureSuccessStatusCode();
        return LoginResult.Success;
    }
}
