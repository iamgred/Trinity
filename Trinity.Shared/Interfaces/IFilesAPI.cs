using Refit;

namespace Trinity.Shared.Interfaces;

public interface IFilesAPI
{
    [Get("/api/v1/files/{campaignId}")]
    Task<List<ReceivedFileInfo>> ListFilesAsync(int campaignId);

    [Get("/api/v1/files/{campaignId}/download/{fileName}")]
    Task<HttpResponseMessage> DownloadFileAsync(int campaignId, string fileName);
}
