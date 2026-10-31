namespace Client.Services;

public enum TaskStatusKind { Success, Failed, Running, Queued, Other }

// the server says SUCCESS/ERROR/..., older code said Successful/Failure, so accept both
public static class TaskStatuses
{
    public static TaskStatusKind Kind(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        "SUCCESS" or "SUCCESSFUL" => TaskStatusKind.Success,
        "ERROR" or "FAILURE" or "FAILED" => TaskStatusKind.Failed,
        "RUNNING" or "DISPATCHED" or "MORE_TO_FOLLOW" or "PENDING" => TaskStatusKind.Running,
        "QUEUED" => TaskStatusKind.Queued,
        _ => TaskStatusKind.Other,
    };

    public static bool IsFailed(string? status) => Kind(status) == TaskStatusKind.Failed;
}
