using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class PayloadError
    {
        public static Error ListenerNotFound(int ID) => new Error(
            "Payloads.ListenerNotFound",
            $"No listener found with ID {ID}.",
            ErrorType.NotFound
        );

        public static Error GenerationFailed() => new Error(
            "Payloads.GenerationFailed",
            $"Payload generation failed.",
            ErrorType.Validation
        );
    }
}