using System.Net.Http.Headers;

namespace Client.Services;

// Scoped per Blazor circuit — reads the token from TeamServerConnection on each request.
public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly TeamServerConnection _connection;

    public AuthTokenHandler(TeamServerConnection connection)
        : base(new HttpClientHandler())
    {
        _connection = connection;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        if (_connection.Token is { } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, ct);
    }
}
