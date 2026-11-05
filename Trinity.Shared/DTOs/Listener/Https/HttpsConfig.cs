namespace Trinity.Shared.DTOs.Listener.Https
{
    public record HttpsConfig
    {
        public int C2Port { get; init; }
        public string UserAgent { get; init; }
        public string Header { get; init; }
        public List<string> Hosts { get; init; }
        public string RotationStrategy { get; init; }
        public string MaxRetryStrategy { get; init; }

        public HttpsConfig(int c2Port, string? userAgent, string? header, List<string> hosts, string rotationStrategy, string maxRetryStrategy)
        {
            C2Port = c2Port;
            UserAgent = userAgent ?? string.Empty;
            Header = header ?? "Content-type: */*";
            RotationStrategy = rotationStrategy;
            MaxRetryStrategy = maxRetryStrategy;
            if (hosts.Count <= 0)
            {
                throw new ArgumentException("A host must be specified!");
            }
            Hosts = hosts;
        }
    }
}
