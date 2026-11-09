namespace Trinity.Shared.DTOs.Authentication;

public sealed class LoginResponse
{
    public required string Token    { get; init; }
    public required string Username { get; init; }
}
