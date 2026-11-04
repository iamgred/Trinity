// ============================================== { START OF FILE } ============================================== //
using Trinity.Shared.Enums;
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IFileStorageService
    {
        // Task<Result<string>> SaveFileAsync(IFormFile file);
        Task<Result<byte[]>> GetFileAsync(string fileName, FileFolders folderName);
        Result<List<string>> ListFiles(FileFolders folderName);
        Task<Result<string>> SaveFileAsync(IFormFile file);
    }
}
// ============================================== { END OF FILE } ============================================== //