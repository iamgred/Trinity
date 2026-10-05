using Microsoft.AspNetCore.Components;

namespace Client.Components.Pages.Login;

public partial class Login
{
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    private bool _showPassword;

    private void HandleLogin()
    {
        Nav.NavigateTo("/dashboard");
    }
}
