// ============================================ { START OF FILE } ============================================ //
using System;
using System.Text;
using Trinity.Shared.Interfaces;

namespace TeamServer.Services.Communication
{
    public class TrinityCommunicationCodec : ICommunicationCodec
    {
        private const int UUID_LENGTH = 36; // Length of a UUID string

        public byte[] Encode(string uuid, ReadOnlyMemory<byte> payload)
        {
            // Convert the UUID to bytes
            byte[] uuidBytes = Encoding.UTF8.GetBytes(uuid);

            // Create a new byte array to hold the encoded data
            byte[] result = new byte[uuidBytes.Length + payload.Length];

            // Copy the UUID bytes and payload into the encoded data array
            Buffer.BlockCopy(uuidBytes, 0, result, 0, uuidBytes.Length);
            // Copy the payload bytes into the encoded data array
            payload.Span.CopyTo(result.AsSpan(uuidBytes.Length));

            // Encode the result as a base64 string
            var encoded = Convert.ToBase64String(result);

            return Encoding.UTF8.GetBytes(encoded);
        }

        public (string uuid, ReadOnlyMemory<byte> payload) Decode(ReadOnlyMemory<byte> data)
        {
            // Decode the base64 string back to bytes
            var base64 = Encoding.UTF8.GetString(data.Span);
            var decoded = Convert.FromBase64String(base64);

            // Validate that the decoded data is long enough to contain a UUID
            if (decoded.Length < UUID_LENGTH)
            {
                throw new FormatException("Communication message is missing the UUID");
            }

            // Extract the UUID and payload from the decoded data
            var uuid = Encoding.UTF8.GetString(decoded, 0, UUID_LENGTH);
            var payload = new ReadOnlyMemory<byte>(decoded, UUID_LENGTH, decoded.Length - UUID_LENGTH);

            return (uuid, payload);
        }
    }
}
// ============================================ { END OF FILE } ============================================ //