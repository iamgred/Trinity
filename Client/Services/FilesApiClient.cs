using Refit;
using Trinity.Shared.Interfaces;

namespace Client.Services;

public sealed record DownloadEntry(string Name, long Size, DateTime LastModified);

public sealed class FilesApiClient(IFilesAPI api, TeamServerConnection connection)
{
    public async Task<List<DownloadEntry>> GetLootFilesAsync(CancellationToken ct = default)
    {
        if (connection.CampaignId is not int campaignId)
            return [];

        try
        {
            var files = await api.ListFilesAsync(campaignId);
            return files.Select(f => new DownloadEntry(f.FileName, f.Size, f.LastModified)).ToList();
        }
        catch (ApiException) { return []; }
    }

    public async Task<string?> GetDownloadUrlAsync(string fileName)
    {
        if (connection.CampaignId is not int campaignId)
            return null;

        try
        {
            using var resp = await api.DownloadFileAsync(campaignId, fileName);
            return resp.RequestMessage?.RequestUri?.ToString();
        }
        catch (ApiException) { return null; }
    }
}
