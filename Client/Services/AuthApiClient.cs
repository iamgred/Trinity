using System.Net;
using Refit;
using Trinity.Shared.DTOs.Authentication;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed class AuthApiClient(IAuthAPI api, TeamServerConnection connection)
{
    public enum LoginResult { Success, InvalidCredentials }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        try
        {
            var response = await api.LoginAsync(new LoginDTO { Username = username, Password = password });
            connection.SetToken(response.Token);
            return LoginResult.Success;
        }
        catch (ApiException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return LoginResult.InvalidCredentials;
        }
    }
}
