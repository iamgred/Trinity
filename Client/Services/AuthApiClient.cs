using System.Net;
using Refit;
using Trinity.Shared.DTOs.Authentication;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed class AuthApiClient(IAuthAPI api)
{
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
    }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            await api.LoginAsync(new LoginDTO { Username = username, Password = password });
            return LoginResult.Success;
        }
        catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return LoginResult.InvalidCredentials;
        }
    }
}
