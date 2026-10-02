// ========================================================= { START OF FILE } =============================================================== //
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class PayloadError
    {
        public static Error GenerationFailed()
        {
            return new Error(
                "Payloads.GenerationFailed",
                "Failed to generate the payload.",
                ErrorType.Validation
            );
        }

        public static Error NotFound(int ID)
        {
            return new Error(
                "Payloads.NotFound",
                $"Payload with ID '{ID}' not found.",
                ErrorType.NotFound
            );
        }
    }
}
// ========================================================= { END OF FILE } =============================================================== //