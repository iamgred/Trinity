//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Trinity.Shared.DTOs.Data;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums;

namespace TeamServer.Services
{
    public class UploadService : IUploadService
    {
        private const int BufferSize = 512 * 1024;

        private readonly string _filesDirectory;

        public UploadService(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            var root = Path.GetFullPath(
                configuration["StoragePath"]
                ?? Path.Combine(environment.ContentRootPath, "storage"));

            _filesDirectory = Path.Combine(root, FileFolders.Loot.ToString());

            Directory.CreateDirectory(_filesDirectory);
        }

        public async Task<Result<bool>> UploadChunkAsync(
            int campaignId,
            UploadChunkDTO chunk)
        {
            if (chunk.Offset < 0)
            {
                return Error.Validation(
                    "Upload.InvalidOffset",
                    "Upload offset cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(chunk.FileName))
            {
                return Error.Validation(
                    "Upload.InvalidFileName",
                    "Upload file name cannot be empty.");
            }

            byte[] bytes;

            try
            {
                bytes = Convert.FromBase64String(chunk.Data);
            }
            catch (FormatException)
            {
                return Error.Validation(
                    "Upload.InvalidBase64",
                    "Upload data is not valid Base64.");
            }

            var campaignDirectory = Path.Combine(
                _filesDirectory,
                campaignId.ToString());

            Directory.CreateDirectory(campaignDirectory);

            var filePath = Path.Combine(
                campaignDirectory,
                Path.GetFileName(chunk.FileName));

            try
            {
                await using var stream = new FileStream(
                    filePath,
                    FileMode.OpenOrCreate,
                    FileAccess.Write,
                    FileShare.Read,
                    BufferSize,
                    useAsync: true);

                stream.Position = chunk.Offset;

                await stream.WriteAsync(bytes);

                return true;
            }
            catch (IOException)
            {
                return Error.Failure(
                    "Upload.WriteFailed",
                    "The upload chunk could not be written.");
            }
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//