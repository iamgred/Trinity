using System.Text.Json;

namespace TeamServer.Interface;

public interface IFileTransferService
{
    // Store a complete file received from C++.
    // Stream is used so the whole file does not have to be held in memory.
    Task ReceiveFileAsync(string agentKey, string transferId, string fileName, int taskId, Stream data, CancellationToken cancellationToken = default);

    // Store one numbered chunk received from C++.
    // The transfer ID groups all chunks that belong to one original file.
    Task ReceiveChunkAsync(string agentKey, string transferId, int taskId, int chunkNumber, Stream data, CancellationToken cancellationToken = default);

    // Join the chunks belonging to a transfer into one output file.
    Task<string> RebuildFileAsync(string agentKey, string transferId, int taskId, long expectedBytes, CancellationToken cancellationToken = default);

    // Append incoming continuous data to a text stream file.
    Task ReceiveStreamDataAsync(string agentKey, string streamId, Stream data, CancellationToken cancellationToken = default);

    Task<JsonDocument> PrepareDownloadChunkAsync(Trinity.Shared.Models.Task task, string agentKey, CancellationToken cancellationToken = default);
}