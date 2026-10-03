// ============================================ { START OF FILE } ============================================ //
namespace Trinity.Shared.Interfaces
{
    public interface ICommunicationCodec
    {
        /// <summary>
        /// Encodes the given payload with the specified UUID into a byte array.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        byte[] Encode(string uuid, ReadOnlyMemory<byte> payload);
        /// <summary>
        /// Decodes the given byte array into a tuple containing the UUID and the payload.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        (string uuid, ReadOnlyMemory<byte> payload) Decode(ReadOnlyMemory<byte> data);
    }
}
// ============================================ { END OF FILE } ============================================ //