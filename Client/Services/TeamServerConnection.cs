namespace Client.Services;

public sealed class TeamServerConnection
{
    public Uri? BaseAddress { get; private set; }

    public void Configure(string address)
    {
        if (!Uri.TryCreate(address.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException("Enter a complete TeamServer URL, such as https://server:50050.");
        }

        BaseAddress = new UriBuilder(uri)
        {
            Path = uri.AbsolutePath.TrimEnd('/') + "/"
        }.Uri;
    }
}
