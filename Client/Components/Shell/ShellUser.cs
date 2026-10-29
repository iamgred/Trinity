namespace Client.Components.Shell;

public sealed record ShellUser(string Name, string Role)
{
    public const string StorageKey = "trinity.shell.user";

    public bool IsAdmin => Role == "admin";
}
