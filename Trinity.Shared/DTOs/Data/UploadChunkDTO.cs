namespace Trinity.Shared.DTOs.Data;

public class UploadChunkDTO
{
    public string FileName { get; set; } = String.Empty;
    public string Data { get; set; } = String.Empty;
    public long Offset { get; set; } = 0;
    public long TotalSize { get; set; } = 0;
    public bool Final { get; set; } = false;
}