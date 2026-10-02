// ============================================================ { START OF FILE } ============================================================ //
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class AdminError
    {
        public static Error NotFound(int adminID)
        {
            return new Error(
                "Admins.NotFound",
                $"Admin with ID {adminID} was not found.",
                ErrorType.NotFound
            );
        }
    }
}
// ============================================================ { END OF FILE } ============================================================ //
