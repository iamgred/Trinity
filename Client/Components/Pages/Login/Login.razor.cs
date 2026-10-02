namespace Client.Components.Pages.Login;
//All of this is mock data for UI testing, can be removed without issue
public partial class Login
{
    private bool _showPassword;

    private void HandleLogin()
    {
        Nav.NavigateTo("/clients");
    }
}
