using Refit;
using Trinity.Shared.DTOs.Authentication;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public class AuthService(IAuthAPI api)
{
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            await api.LoginAsync(new LoginDTO { Username = username, Password = password });
            return true;
        }
        catch (ApiException) { return false; }
        catch (Exception)    { return false; }
    }
}
