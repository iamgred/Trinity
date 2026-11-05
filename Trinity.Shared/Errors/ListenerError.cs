//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class ListenerError
    {
        public static Error NotFound(int ID) => new Error("Listeners.NotFound", $"The listener with Id '{ID}' was not found", ErrorType.NotFound);
        public static Error ModuleNotFound(int moduleID) => new Error("Listeners.ModuleNotFound", $"Could not find HTTP module with Id '{moduleID}'", ErrorType.NotFound);
        public static Error PortOccupied(int port) => new Error("Listeners.PortOccupied", $"Could not start listener, port '{port}' is occupied", ErrorType.Conflict);
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//