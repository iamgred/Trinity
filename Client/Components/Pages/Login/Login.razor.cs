using Client.Components.Shell;
using Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Trinity.Shared.Interfaces;

namespace Client.Components.Pages.Login;

public partial class Login
{
    // Nav comes from Login.razor

    [Inject]
    private TeamServerConnection TeamServerConnection { get; set; } = default!;

    [Inject]
    private AuthApiClient AuthApiClient { get; set; } = default!;

    [Inject]
    private AdminService AdminService { get; set; } = default!;

    [Inject]
    private ICampaignAPI CampaignAPI { get; set; } = default!;

    [Inject]
    private ProtectedSessionStorage Session { get; set; } = default!;

    [Inject]
    private IConfiguration Config { get; set; } = default!;

    private string _username = "";
    private string _password = "";
    private string? _error;
    private bool _showPassword;
    private bool _isSigningIn;

    protected override void OnInitialized()
    {
        if (TeamServerConnection.Token is not null)
            Nav.NavigateTo("/agents", replace: true);
    }

    private async Task HandleLogin()
    {
        _error = null;

        try
        {
            // server address comes from appsettings
            TeamServerConnection.Configure(Config["TeamServer:BaseUrl"] ?? "");
        }
        catch (ArgumentException exception)
        {
            _error = $"TeamServer:BaseUrl in appsettings is not valid. {exception.Message}";
            return;
        }

        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrEmpty(_password))
        {
            _error = "Enter both a username and a password.";
            return;
        }

        _isSigningIn = true;
        try
        {
            var result = await AuthApiClient.LoginAsync(_username, _password);

            if (result == AuthApiClient.LoginResult.InvalidCredentials)
            {
                _error = "Invalid username or password.";
                return;
            }

            // forget the password once its sent
            _password = "";

            await TrySetCampaignIdAsync();

            var user = new ShellUser(_username.Trim(), await IsAdminAsync(_username.Trim()) ? "admin" : "operator");
            await Session.SetAsync(ShellUser.StorageKey, user);
            Nav.NavigateTo("agents");
        }
        catch (HttpRequestException)
        {
            _error = "Could not reach the team server. Check that it is running.";
        }
        finally
        {
            _isSigningIn = false;
        }
    }

    private async Task TrySetCampaignIdAsync()
    {
        try
        {
            var campaigns = await CampaignAPI.GetMyCampaignsAsync();
            if (campaigns is { Count: > 0 })
                TeamServerConnection.SetCampaignId(campaigns[0].ID);
        }
        catch (HttpRequestException) { }
    }

    // TODO(connect to backend): login doesn't say if you're an admin, so check the admin list
    private async Task<bool> IsAdminAsync(string username)
    {
        try
        {
            var admins = await AdminService.GetTasksAsync();
            return admins.Any(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
