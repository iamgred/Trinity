using Trinity.Shared.Enums.Tasks;

namespace Trinity.Shared.DTOs.Notification
{
    public record TaskNotification(DateTime Timestamp, int TaskID, string message, Status Status);
}
