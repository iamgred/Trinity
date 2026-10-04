// ============================================== { START OF FILE } ============================================== //
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Models;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IPayloadBuilder
    {
        Task<Result<string>> BuildAsync(PayloadCreationDTO payloadCreationDTO, string payloadUUID, Listener listener);
    }
}
// ============================================== { END OF FILE } ============================================== //