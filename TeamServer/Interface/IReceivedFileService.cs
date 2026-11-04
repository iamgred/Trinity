namespace TeamServer.Interface;

// Provides read-only operations for files already stored by the C# application.
public interface IReceivedFileService
{
    // Return metadata such as name, size, and modification time for each file.
    Task<IReadOnlyList<ReceivedFileInfo>> ListFilesAsync(int campaignId);

    // Open one file for reading so ASP.NET Core can stream it to a downloader.
    Task<FileStream?> OpenFileAsync(int campaignId, string fileName);
}

// A small data-transfer object used in the JSON response from GET /api/files.
// A record is useful here because this object is only a value describing a file.
public sealed record ReceivedFileInfo(string FileName, long Size, DateTime LastModified);
