using Client.Services;
using Microsoft.AspNetCore.Components;

namespace Client.Components.Pages.Login;

public partial class Login
{
    [Inject] private AuthService AuthService { get; set; } = default!;
    // Nav is already injected via @inject in Login.razor — don't redeclare it here

    private string _serverAddress = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _showPassword;
    private bool _isLoading;
    private string? _errorMessage;

    private async Task HandleLogin()
    {
        // DEV BYPASS — remove these two lines when TeamServer is ready
        Nav.NavigateTo("/dashboard");
        return;

        _isLoading = true;
        _errorMessage = null;

        bool success = await AuthService.LoginAsync(_username, _password);

        if (success)
            Nav.NavigateTo("/dashboard");
        else
            _errorMessage = "Invalid username or password.";

        _isLoading = false;
    }
}