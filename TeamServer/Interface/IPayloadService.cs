// ============================================== { START OF FILE} ============================================== //
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IPayloadService
    {
        Task<Result> GeneratePayloadAsync(PayloadCreationDTO payloadCreationDTO);
        Task<Result<List<PayloadDTO>>> GetPayloadsAsync();
        Task<Result<string>> GetPayloadFilePathAsync(int payloadID);
    }
}
// ============================================== { END OF FILE} ============================================== //