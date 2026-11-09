using Refit;

namespace Trinity.Shared.Interfaces;

public interface IFilesAPI
{
    [Get("/api/v1/files")]
    Task<List<string>> ListFilesAsync([Query] string folderName);

    [Get("/api/v1/files/download/{fileName}")]
    Task<HttpResponseMessage> DownloadFileAsync(string fileName, [Query] string folderName);
}
