// ================================================================== { START OF FILE } =================================================================== //
namespace Trinity.Shared.Interfaces
{
    public interface ICommunicationTransport
    {
        /// <summary>
        /// Connects to the communication endpoint.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ConnectAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Sends the specified data to the communication endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>
        /// The data is sent as a byte array.
        /// </remarks>
        Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);
        /// <summary>
        /// Receives data from the communication endpoint.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Disconnects from the communication endpoint.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DisconnectAsync(CancellationToken cancellationToken = default);
    }
}
// ================================================================== { END OF FILE } =================================================================== //