using Refit;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record DownloadEntry(string Name, string Path);

public sealed class FilesApiClient(IFilesAPI api)
{
    public async Task<List<DownloadEntry>> GetLootFilesAsync(CancellationToken ct = default)
    {
        try
        {
            var paths = await api.ListFilesAsync("Loot");
            return paths.Select(p => new DownloadEntry(System.IO.Path.GetFileName(p), p)).ToList();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            return [];
        }
        catch (ApiException) { return []; }
    }

    public async Task<string?> GetDownloadUrlAsync(string fileName, string folderName = "Loot")
    {
        try
        {
            using var resp = await api.DownloadFileAsync(fileName, folderName);
            return resp.RequestMessage?.RequestUri?.ToString();
        }
        catch (ApiException) { return null; }
    }
}
