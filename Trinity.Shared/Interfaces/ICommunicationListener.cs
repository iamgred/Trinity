// ================================================================== { START OF FILE } =================================================================== //
namespace Trinity.Shared.Interfaces
{
    public interface ICommunicationListener
    {
        /// <summary>
        /// Starts the listener and begins accepting incoming connections.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Stops the listener and closes any active connections.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
// ================================================================== { END OF FILE } =================================================================== //