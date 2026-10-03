// =========================================== { START OF FILE } =========================================== //
namespace Trinity.Shared.Interfaces
{
    public interface ICommunicationMessageHandler
    {
        Task<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> message, CancellationToken cancellationToken = default);
    }
}
// =========================================== { END OF FILE } =========================================== //