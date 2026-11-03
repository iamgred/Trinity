namespace Trinity.Shared.DTOs.Task;

public sealed class TaskFileChunkResponse
{
    public required string FileName { get; set; }
    public required string Data { get; set; }
    public long Offset { get; set; }
    public long TotalBytes { get; set; }
    public bool Final { get; set; }
}
