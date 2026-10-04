// ================================================= { START OF FILE } ================================================= //
using Trinity.Shared.Enums;

namespace Trinity.Shared.Errors
{
    public static class FileError
    {
        public static Error EmptyFile()
        {
            return new Error(
                "Files.EmptyFile",
                "The file is null or empty.",
                ErrorType.Validation
            );
        }

        public static Error UploadFailed(string fileName)
        {
            return new Error(
                "Files.UploadFailed",
                $"Failed to upload file '{fileName}'.",
                ErrorType.Failure
            );
        }

        public static Error FileNotFound(string fileName)
        {
            return new Error(
                "Files.FileNotFound",
                $"File '{fileName}' not found.",
                ErrorType.NotFound
            );
        }
    }
}
// ================================================= { END OF FILE } ================================================= //