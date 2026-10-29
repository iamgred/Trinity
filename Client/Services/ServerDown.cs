namespace Client.Services;

// shown instead of the error when the server can't be reached
public static class ServerDown
{
    public const string Message = "Server down";

    // no reply at all = down; an error reply means it's up
    public static bool Is(Exception e) => e is HttpRequestException { StatusCode: null } or TaskCanceledException;
}
