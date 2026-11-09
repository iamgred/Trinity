using Refit;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record DownloadEntry(string Name, long Size, DateTime LastModified);

public sealed class FilesApiClient(IFilesAPI api)
{
    public async Task<List<DownloadEntry>> GetLootFilesAsync(CancellationToken ct = default)
    {
        try
        {
            var files = await api.ListFilesAsync();
            return files.Select(f => new DownloadEntry(f.FileName, f.Size, f.LastModified)).ToList();
        }
        catch (ApiException) { return []; }
    }

    public async Task<string?> GetDownloadUrlAsync(string fileName)
    {
        try
        {
            using var resp = await api.DownloadFileAsync(fileName);
            return resp.RequestMessage?.RequestUri?.ToString();
        }
        catch (ApiException) { return null; }
    }
}
