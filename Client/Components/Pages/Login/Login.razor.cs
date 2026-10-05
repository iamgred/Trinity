using Microsoft.AspNetCore.Components;
using Client.Services;

namespace Client.Components.Pages.Login;

public partial class Login
{
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Inject]
    private TeamServerConnection TeamServerConnection { get; set; } = default!;

    [Inject]
    private AuthApiClient AuthApiClient { get; set; } = default!;

    private string _teamServerUrl = "";
    private string _username = "";
    private string _password = "";
    private string? _connectionError;
    private bool _showPassword;
    private bool _isSigningIn;

    private async Task HandleLogin()
    {
        _connectionError = null;

        try
        {
            TeamServerConnection.Configure(_teamServerUrl);
        }
        catch (ArgumentException exception)
        {
            _connectionError = exception.Message;
            return;
        }

        if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrEmpty(_password))
        {
            _connectionError = "Enter both an operator name and a password.";
            return;
        }

        _isSigningIn = true;
        try
        {
            var result = await AuthApiClient.LoginAsync(_username, _password);

            if (result == AuthApiClient.LoginResult.InvalidCredentials)
            {
                _connectionError = "Invalid operator name or password.";
                return;
            }

            // Do not retain the password beyond the request.
            _password = "";
            Nav.NavigateTo("/dashboard");
        }
        catch (HttpRequestException)
        {
            _connectionError = "Could not reach the TeamServer. Check the URL and that the server is running.";
        }
        finally
        {
            _isSigningIn = false;
        }
    }
}
