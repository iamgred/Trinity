using TeamServer.Interface;

namespace TeamServer.Services;

public sealed class FileTransferService : IFileTransferService
{
    // Files smaller than 100 MiB are sent as one HTTP request.
    // This keeps small transfers simple and avoids unnecessary chunk bookkeeping.
    private const long FileThreshold = 100L * 1024 * 1024;

    // Files at or above the threshold are divided into 10 MiB chunks.
    // A fixed chunk size limits memory usage while transferring large files.
    private const int ChunkSize = 10 * 1024 * 1024;

    // HttpClient performs outbound C# -> C++ HTTP calls.
    private readonly HttpClient _httpClient;

    // Absolute root folder used for received files, chunks, and streams.
    private readonly string _storageRoot;

    public FileTransferService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Ask the factory for a configured HttpClient.
        // The factory manages the underlying connections more safely than creating
        // a new HttpClient for every file or chunk.
        _httpClient = httpClientFactory.CreateClient();

        // Read StoragePath from configuration.
        // If it is missing, store data in a storage folder beside the application.
        _storageRoot = Path.GetFullPath(configuration["StoragePath"] ?? Path.Combine(environment.ContentRootPath, "storage"));

        // Create the normal-file directory when the service starts.
        Directory.CreateDirectory(Path.Combine(_storageRoot, "files"));

        // Create the continuous-stream directory when the service starts.
        Directory.CreateDirectory(Path.Combine(_storageRoot, "streams"));
    }

    public async Task SendFileAsync(string filePath, string cppUrl, CancellationToken cancellationToken = default)
    {
        // FileInfo provides the source file's existence and size.
        var fileInfo = new FileInfo(filePath);

        // Stop early with a useful error instead of making an invalid HTTP request.
        if (!fileInfo.Exists)
            throw new FileNotFoundException("File was not found.", filePath);

        // Give this transfer a unique ID.
        // For a large file, this ID groups all chunk requests together.
        var transferId = Guid.NewGuid().ToString("N");

        // Remove a possible trailing slash so URL paths do not contain "//".
        var baseUrl = cppUrl.TrimEnd('/');

        // Small files use one request because chunking would add unnecessary work.
        if (fileInfo.Length < FileThreshold)
        {
            // Open the source in binary mode.
            // await using closes the file even if the HTTP call fails.
            await using var file = File.OpenRead(filePath);

            // Wrap the file stream as an HTTP request body.
            using var content = new StreamContent(file);

            // Send the original filename in a custom header.
            // The C++ server uses it when naming the received file.
            content.Headers.Add("X-File-Name", fileInfo.Name);

            // POST the complete file to the C++ server.
            using var response = await _httpClient.PostAsync($"{baseUrl}/api/files/{transferId}/file", content, cancellationToken);

            // Throw if the C++ server rejected the request.
            response.EnsureSuccessStatusCode();
            return;
        }

        // Open a large source file once and read it incrementally.
        await using var input = File.OpenRead(filePath);

        // Reuse one buffer for every chunk to keep memory consumption predictable.
        var buffer = new byte[ChunkSize];

        // The first chunk is numbered zero.
        var chunkNumber = 0;

        // This variable receives the number of bytes read on each iteration.
        int bytesRead;

        // Read until ReadAsync returns zero, which means end-of-file.
        while ((bytesRead = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            // Send only the valid part of the buffer.
            // The final chunk may be smaller than 10 MiB.
            using var content = new ByteArrayContent(buffer, 0, bytesRead);

            // POST this chunk to its numbered endpoint.
            using var response = await _httpClient.PostAsync(
                $"{baseUrl}/api/files/{transferId}/chunks/{chunkNumber}", content, cancellationToken);

            // Do not continue if the receiver rejected a chunk.
            response.EnsureSuccessStatusCode();

            // Prepare the number for the next chunk.
            chunkNumber++;
        }

        // Tell the C++ server that every chunk has been sent.
        // The receiver can now rebuild the original file.
        using var rebuildResponse = await _httpClient.PostAsync(
            $"{baseUrl}/api/files/{transferId}/rebuild", content: null, cancellationToken);
        rebuildResponse.EnsureSuccessStatusCode();
    }

    public Task ReceiveFileAsync(string fileName, Stream data, CancellationToken cancellationToken = default) =>
        // Store a complete C++ -> C# file in the files directory.
        CopyToFileAsync(Path.Combine(_storageRoot, "files", SafeName(fileName)), data, cancellationToken);

    public Task ReceiveChunkAsync(string transferId, int chunkNumber, Stream data, CancellationToken cancellationToken = default)
    {
        // A negative number is invalid and would not represent a real chunk.
        if (chunkNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(chunkNumber));

        // Get or create this transfer's private chunk directory.
        var directory = TransferDirectory(transferId);

        // Zero padding ensures chunk_00000002 sorts after chunk_00000001.
        return CopyToFileAsync(Path.Combine(directory, $"chunk_{chunkNumber:D8}"), data, cancellationToken);
    }

    public async Task<string> RebuildFileAsync(string transferId, CancellationToken cancellationToken = default)
    {
        // Locate the directory containing this transfer's chunks.
        var directory = TransferDirectory(transferId);

        // Rebuilding cannot work if the directory does not exist.
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException("Transfer directory was not found.");

        // Find only chunk files and sort them into their original order.
        var chunks = Directory.EnumerateFiles(directory, "chunk_*").OrderBy(path => path, StringComparer.Ordinal).ToArray();

        // An empty directory indicates that no data was received.
        if (chunks.Length == 0)
            throw new InvalidOperationException("No chunks were found.");

        // Store the rebuilt result beside other received files.
        var outputPath = Path.Combine(_storageRoot, "files", $"{SafeName(transferId)}_rebuilt");

        // Create the destination in binary mode.
        await using var output = File.Create(outputPath);
        foreach (var chunk in chunks)
        {
            // Open one chunk at a time to avoid loading the entire transfer in memory.
            await using var input = File.OpenRead(chunk);

            // Copy bytes unchanged so JPG, ZIP, PDF, and other binary files work.
            await input.CopyToAsync(output, cancellationToken);
        }

        return outputPath;
    }

    public Task ReceiveStreamDataAsync(string streamId, Stream data, CancellationToken cancellationToken = default) =>
        // Append because continuous data arrives in multiple HTTP requests.
        CopyToFileAsync(Path.Combine(_storageRoot, "streams", $"{SafeName(streamId)}.txt"), data, cancellationToken, FileMode.Append);

    private string TransferDirectory(string transferId)
    {
        // Use a separate directory for every transfer.
        var safeTransferId = SafeName(transferId);
        var directory = Path.Combine(_storageRoot, "files", safeTransferId);

        // Create the directory before the caller writes a chunk.
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static async Task CopyToFileAsync(string path, Stream data, CancellationToken cancellationToken, FileMode mode = FileMode.Create)
    {
        // Ensure the target directory exists before opening the file.
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // FileMode.Create replaces an existing file; Append adds to the end.
        await using var output = new FileStream(path, mode, FileAccess.Write, FileShare.Read);

        // Stream.CopyToAsync transfers bytes without loading the complete body.
        await data.CopyToAsync(output, cancellationToken);
    }

    private static string SafeName(string name)
    {
        // Decode URL characters and keep only the final filename component.
        // This blocks values such as ../../secret.txt from escaping storage.
        var safeName = Path.GetFileName(Uri.UnescapeDataString(name));

        // Reject empty, current-directory, and parent-directory names.
        if (string.IsNullOrWhiteSpace(safeName) || safeName is "." or "..")
            throw new ArgumentException("A safe, non-empty name is required.", nameof(name));
        return safeName;
    }
}