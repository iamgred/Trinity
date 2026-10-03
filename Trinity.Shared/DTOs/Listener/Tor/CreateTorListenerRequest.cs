// ======================================== { START OF FILE } ======================================== //
using System.Text.Json.Serialization;

namespace Trinity.Shared.DTOs.Listener.Tor
{
    public record CreateTorListenerRequest(
        string Name,
        TorConfig Config
    );

    public record TorConfig(
        string Endpoint,
        int Port
    );
}
// ======================================== { START OF FILE } ======================================== //