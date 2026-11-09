using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;
using Trinity.Shared.Interfaces;

namespace TeamServer.Services
{
    public class DownloadService : IDownloadService
    {
        private const int BufferSize = 512 * 1024;

        public async Task<Result<DownloadDTO>> DownloadChunkAsync(
            string path,
            string destinationPath,
            long offset)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Error.Validation(
                    "Download.InvalidPath",
                    "Download source path cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(destinationPath))
            {
                return Error.Validation(
                    "Download.InvalidDestinationPath",
                    "Download destination path cannot be empty.");
            }

            if (offset < 0)
            {
                return Error.Validation(
                    "Download.InvalidOffset",
                    "Download offset cannot be negative.");
            }

            try
            {
                await using var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    BufferSize,
                    useAsync: true);

                if (offset > stream.Length)
                {
                    return Error.Validation(
                        "Download.InvalidOffset",
                        "Download offset is beyond the end of the file.");
                }

                stream.Position = offset;

                var remaining = stream.Length - offset;

                var bytesToRead = (int)Math.Min(
                    BufferSize,
                    remaining);

                var buffer = new byte[bytesToRead];

                var bytesRead = await stream.ReadAsync(buffer);

                var final =
                    offset + bytesRead >= stream.Length;

                var data = Convert.ToBase64String(
                    buffer,
                    0,
                    bytesRead);

                return new DownloadDTO
                {
                    Path = path,
                    DestinationPath = destinationPath,
                    Data = data,
                    Offset = offset,
                    Final = final
                };
            }
            catch (FileNotFoundException)
            {
                return Error.NotFound(
                    "Download.FileNotFound",
                    "The download source file could not be found.");
            }
            catch (DirectoryNotFoundException)
            {
                return Error.NotFound(
                    "Download.DirectoryNotFound",
                    "The download source directory could not be found.");
            }
            catch (IOException)
            {
                return Error.Failure(
                    "Download.ReadFailed",
                    "The download source file could not be read.");
            }
        }
    }
}