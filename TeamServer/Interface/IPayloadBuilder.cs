// ============================================== { START OF FILE } ============================================== //
using System.Text.Json;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IPayloadBuilder
    {
        Task<Result<string>> BuildAsync(
            PayloadCreationDTO payloadCreationDTO,
            JsonDocument listenerConfig);
    }
}
// ============================================== { END OF FILE } ============================================== //