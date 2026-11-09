// ======================================= { START OF FILE } ======================================= //
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

namespace TeamServer.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileStorageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Retrieves a file from the storage/uploads directory.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<Result<byte[]>> GetFileAsync(string fileName,FileFolders folderName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return FileError.FileNotFound(fileName);
            }

            // Check if the file exists in the storage/uploads directory
            var storagePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "storage",
                folderName.ToString());

            var safeFileName = Path.GetFileName(fileName);

            var filePath = Path.Combine(
                storagePath,
                safeFileName);

            if (!File.Exists(filePath))
            {
                return FileError.FileNotFound(safeFileName);
            }

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            return fileBytes;
        }

        /// <summary>
        /// Lists all files in the storage/uploads directory.
        /// </summary>
        /// <returns></returns>
        public Result<List<string>> ListFiles(FileFolders folderName)
        {
            // Ensure the storage/uploads directory exists
            var storagePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "storage",
                folderName.ToString());

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            // Get all files in the storage/uploads directory
            var files = Directory.GetFiles(storagePath).ToList();

            if (files.Count == 0)
            {
                return FileError.FileNotFound("No files found.");
            }

            return files;
        }

        /// <summary>
        /// Saves a file to the storage/uploads directory.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>The stored file name.</returns>
        public async Task<Result<string>> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return FileError.EmptyFile();
            }

            // Ensure the storage/uploads directory exists
            var storagePath = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "storage",
                FileFolders.Uploads.ToString());

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            // Strip any directory information supplied by the client.
            var fileName = Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return FileError.EmptyFile();
            }

            var fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension(fileName);

            var extension =
                Path.GetExtension(fileName);

            var destinationPath = Path.Combine(
                storagePath,
                fileName);

            var counter = 1;

            // Avoid overwriting an existing file.
            while (File.Exists(destinationPath))
            {
                var newFileName =
                    $"{fileNameWithoutExtension}({counter}){extension}";

                destinationPath = Path.Combine(
                    storagePath,
                    newFileName);

                counter++;
            }

            // Save the file to the storage/uploads directory
            await using var stream = new FileStream(
                destinationPath,
                FileMode.CreateNew);

            await file.CopyToAsync(stream);

            return Path.GetFileName(destinationPath);
        }
    }
}
// ======================================= { END OF FILE } ======================================= //