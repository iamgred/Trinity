using TeamServer.Interface;

namespace TeamServer.Services;

public sealed class ReceivedFileService : IReceivedFileService
{
    // This service lists completed/top-level files, not chunk directories.
    private readonly string _filesDirectory;

    public ReceivedFileService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Resolve the same configured storage location used by FileTransferService.
        var root = Path.GetFullPath(configuration["StoragePath"] ?? Path.Combine(environment.ContentRootPath, "storage"));
        _filesDirectory = Path.Combine(root, "files");
        Directory.CreateDirectory(_filesDirectory);
    }

    public Task<IReadOnlyList<ReceivedFileInfo>> ListFilesAsync()
    {
        // Enumerate only direct files.
        // Chunk directories are intentionally excluded from the public list.
        var files = Directory.EnumerateFiles(_filesDirectory)
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.Name, StringComparer.Ordinal)
            .Select(file => new ReceivedFileInfo(file.Name, file.Length, file.LastWriteTimeUtc))
            .ToArray();
        return Task.FromResult<IReadOnlyList<ReceivedFileInfo>>(files);
    }

    public Task<FileStream?> OpenFileAsync(string fileName)
    {
        // Strip directory components before combining with the storage path.
        // This prevents a download request from navigating outside storage.
        var safeName = Path.GetFileName(Uri.UnescapeDataString(fileName));
        if (string.IsNullOrWhiteSpace(safeName) || safeName is "." or "..")
            return Task.FromResult<FileStream?>(null);

        // Return null so the controller can produce a normal HTTP 404 response.
        var path = Path.Combine(_filesDirectory, safeName);
        if (!File.Exists(path))
            return Task.FromResult<FileStream?>(null);

        // Open read-only and allow other readers to access the same file.
        return Task.FromResult<FileStream?>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }
}
