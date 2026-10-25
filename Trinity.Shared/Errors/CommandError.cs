//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public class CommandError
    {
        public static Error TooFewArgs(string keyword) => new Error("Commands.TooFewArgs", $"{keyword} error: not enough arguements", ErrorType.Validation);
        public static Error TooManyArgs(string keyword) => new Error("Commands.TooManyArgs", $"{keyword} error: too many arguements", ErrorType.Validation);
        public static Error InvalidHost(string keyword, string host) => new Error("Commands.InvalidHost", $"{keyword} error: invalid host specified '{host}'", ErrorType.Validation);
        public static Error NotFound(string keyword) => new Error("Commands.NotFound", $"Unknown command: '{keyword}'", ErrorType.NotFound);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//