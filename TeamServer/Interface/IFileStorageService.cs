// ============================================== { START OF FILE } ============================================== //
using Trinity.Shared.Results;

namespace TeamServer.Interface
{
    public interface IFileStorageService
    {
        Task<Result<string>> SaveFileAsync(IFormFile file);
        Task<Result<byte[]>> GetFileAsync(string fileName);
        Result<List<string>> ListFiles();
    }
}
// ============================================== { END OF FILE } ============================================== //