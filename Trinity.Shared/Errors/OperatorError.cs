using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class OperatorError
    {
        public static Error NotFound(int operatorID)
        {
            return new Error(
                "Operators.NotFound",
                $"Operator with ID {operatorID} was not found.",
                ErrorType.NotFound
            );
        }
    }
}