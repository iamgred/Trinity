using System.Text.Json;

namespace TeamServer.Interface;

public interface IFileTransferService
{
    Task ReceiveFileAsync(string agentKey, string transferId, string fileName, int taskId, Stream data, CancellationToken cancellationToken = default);
    Task ReceiveChunkAsync(string agentKey, string transferId, int taskId, int chunkNumber, Stream data, CancellationToken cancellationToken = default);
    Task<string> RebuildFileAsync(string agentKey, string transferId, int taskId, long expectedBytes, CancellationToken cancellationToken = default);
    Task ReceiveStreamDataAsync(string agentKey, string streamId, Stream data, CancellationToken cancellationToken = default);

    Task<JsonDocument> PrepareDownloadChunkAsync(Trinity.Shared.Models.Task task, string agentKey, CancellationToken cancellationToken = default);

    Task ReceiveCheckinUploadChunkAsync(
        int taskId,
        string fileName,
        long offset,
        long totalBytes,
        string data,
        bool final,
        CancellationToken cancellationToken = default);
}