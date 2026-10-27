namespace TeamServer.Interface;

public interface IFileTransferService
{
    // Send a local file from C# to the C++ HTTP server.
    // The implementation chooses one request or multiple chunks based on file size.
    Task SendFileAsync(string filePath, string cppUrl, CancellationToken cancellationToken = default);

    // Store a complete file received from C++.
    // Stream is used so the whole file does not have to be held in memory.
    Task ReceiveFileAsync(string fileName, Stream data, CancellationToken cancellationToken = default);

    // Store one numbered chunk received from C++.
    // The transfer ID groups all chunks that belong to one original file.
    Task ReceiveChunkAsync(string transferId, int chunkNumber, Stream data, CancellationToken cancellationToken = default);

    // Join the chunks belonging to a transfer into one output file.
    Task<string> RebuildFileAsync(string transferId, CancellationToken cancellationToken = default);

    // Append incoming continuous data to a text stream file.
    Task ReceiveStreamDataAsync(string streamId, Stream data, CancellationToken cancellationToken = default);
}