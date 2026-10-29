using TeamServer.Interface;
using TeamServer.Repositories;
using System.Text.Json;
using Trinity.Shared.Enums.Tasks;

namespace TeamServer.Services;

public sealed class FileTransferService : IFileTransferService
{
    // Absolute root folder used for received files, chunks, and streams.
    private readonly string _storageRoot;
    private readonly AgentRepository _agentRepository;
    private readonly TaskRepository _taskRepository;

    public FileTransferService(IConfiguration configuration,
        IWebHostEnvironment environment, AgentRepository agentRepository, TaskRepository taskRepository)
    {
        _agentRepository = agentRepository;
        _taskRepository = taskRepository;

        // Read StoragePath from configuration.
        // If it is missing, store data in a storage folder beside the application.
        _storageRoot = Path.GetFullPath(configuration["StoragePath"] ?? Path.Combine(environment.ContentRootPath, "storage"));

        // Create the normal-file directory when the service starts.
        Directory.CreateDirectory(Path.Combine(_storageRoot, "files"));

        // Create the continuous-stream directory when the service starts.
        Directory.CreateDirectory(Path.Combine(_storageRoot, "streams"));
    }

    public async Task ReceiveFileAsync(string agentKey, string transferId, string fileName, int taskId, Stream data, CancellationToken cancellationToken = default)
    {
        var campaignId = await ValidateUploadAsync(agentKey, taskId);
        await CopyToFileAsync(CampaignFilePath(campaignId, fileName), data, cancellationToken);
    }

    public async Task ReceiveChunkAsync(string agentKey, string transferId, int taskId, int chunkNumber, Stream data, CancellationToken cancellationToken = default)
    {
        // A negative number is invalid and would not represent a real chunk.
        if (chunkNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(chunkNumber));

        var campaignId = await ValidateUploadAsync(agentKey, taskId);
        var directory = TransferDirectory(campaignId, transferId);

        // Zero padding ensures chunk_00000002 sorts after chunk_00000001.
        await CopyToFileAsync(Path.Combine(directory, $"chunk_{chunkNumber:D8}"), data, cancellationToken);
    }

    public async Task<string> RebuildFileAsync(string agentKey, string transferId, int taskId, long expectedBytes, CancellationToken cancellationToken = default)
    {
        if (expectedBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedBytes));

        // Locate the directory containing this transfer's chunks.
        var campaignId = await ValidateUploadAsync(agentKey, taskId);
        var directory = TransferDirectory(campaignId, transferId);

        // Rebuilding cannot work if the directory does not exist.
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException("Transfer directory was not found.");

        // Find only chunk files and sort them into their original order.
        var chunks = Directory.EnumerateFiles(directory, "chunk_*")
            .Select(path => new
            {
                Path = path,
                Number = ParseChunkNumber(path)
            })
            .OrderBy(chunk => chunk.Number)
            .ToArray();

        if (chunks.Length == 0)
            throw new InvalidOperationException("No chunks were found.");
        if (chunks[0].Number != 0 || chunks.Select((chunk, index) => chunk.Number == index).Any(valid => !valid))
            throw new InvalidOperationException("Transfer contains missing or duplicate chunks.");

        // Store the rebuilt result beside other received files.
        var outputPath = Path.Combine(_storageRoot, "files", SafeName(campaignId.ToString()),
            $"{SafeName(transferId)}_rebuilt");

        // Create the destination in binary mode.
        long totalBytes = 0;
        await using (var output = File.Create(outputPath))
        {
            foreach (var chunk in chunks)
            {
                await using var input = File.OpenRead(chunk.Path);
                totalBytes += input.Length;
                await input.CopyToAsync(output, cancellationToken);
            }
        }
        if (totalBytes != expectedBytes)
        {
            File.Delete(outputPath);
            throw new InvalidOperationException("Rebuilt file size does not match the expected size.");
        }
        Directory.Delete(directory, recursive: true);

        return outputPath;
    }

    public async Task ReceiveStreamDataAsync(string agentKey, string streamId, Stream data, CancellationToken cancellationToken = default)
    {
        var campaignId = await GetCampaignIdAsync(agentKey);
        await CopyToFileAsync(Path.Combine(_storageRoot, "streams", SafeName(campaignId.ToString()),
            $"{SafeName(streamId)}.txt"), data, cancellationToken, FileMode.Append);
    }

    public async Task<JsonDocument> PrepareDownloadChunkAsync(Trinity.Shared.Models.Task task, string agentKey, CancellationToken cancellationToken = default)
    {
        var command = task.Command.RootElement;
        var fileName = command.GetProperty("Path").GetString() ?? throw new InvalidOperationException("Download path is required.");
        var campaignId = await GetCampaignIdAsync(agentKey);
        var path = CampaignFilePath(campaignId, fileName);
        if (!File.Exists(path))
            throw new FileNotFoundException("The requested file was not found.", fileName);

        var offset = command.TryGetProperty("Offset", out var offsetElement) ? offsetElement.GetInt64() : 0;
        const int chunkSize = 512 * 1024;
        await using var input = File.OpenRead(path);
        if (offset < 0 || offset > input.Length)
            throw new InvalidOperationException("The transfer offset is invalid.");

        input.Position = offset;
        var buffer = new byte[(int)Math.Min(chunkSize, input.Length - offset)];
        var count = await input.ReadAsync(buffer.AsMemory(), cancellationToken);
        var final = offset + count >= input.Length;
        task.Command = JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            Path = fileName,
            Offset = offset + count,
            TotalBytes = input.Length,
            Final = final
        }));
        task.Status = Status.QUEUED;

        return JsonDocument.Parse(JsonSerializer.Serialize(new
        {
            Path = fileName,
            Data = Convert.ToBase64String(buffer, 0, count),
            Offset = offset,
            TotalBytes = input.Length,
            Final = final
        }));
    }

    private string TransferDirectory(int campaignId, string transferId)
    {
        // Use a separate directory for every transfer.
        var directory = Path.Combine(_storageRoot, "files", SafeName(campaignId.ToString()), SafeName(transferId));

        // Create the directory before the caller writes a chunk.
        Directory.CreateDirectory(directory);
        return directory;
    }

    private string CampaignFilePath(int campaignId, string fileName) =>
        Path.Combine(_storageRoot, "files", SafeName(campaignId.ToString()), SafeName(fileName));

    private async Task<int> GetCampaignIdAsync(string agentKey)
    {
        var agent = await _agentRepository.GetAgentByCheckInUUID(agentKey);
        if (agent is null)
            throw new KeyNotFoundException($"Agent '{agentKey}' was not found.");
        return agent.CampaignID;
    }

    private async Task<int> ValidateUploadAsync(string agentKey, int taskId)
    {
        var agent = await _agentRepository.GetAgentByCheckInUUID(agentKey);
        if (agent is null)
            throw new KeyNotFoundException($"Agent '{agentKey}' was not found.");

        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task is null || task.AgentID != agent.ID || task.CommandType != TaskType.UPLOAD)
            throw new UnauthorizedAccessException("Upload task does not belong to this agent.");
        return agent.CampaignID;
    }

    private static async Task CopyToFileAsync(string path, Stream data, CancellationToken cancellationToken, FileMode mode = FileMode.Create)
    {
        // Ensure the target directory exists before opening the file.
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // FileMode.Create replaces an existing file; Append adds to the end.
        await using var output = new FileStream(path, mode, FileAccess.Write, FileShare.Read);

        // Stream.CopyToAsync transfers bytes without loading the complete body.
        await data.CopyToAsync(output, cancellationToken);
    }

    private static string SafeName(string name)
    {
        // Decode URL characters and keep only the final filename component.
        // This blocks values such as ../../secret.txt from escaping storage.
        var safeName = Path.GetFileName(Uri.UnescapeDataString(name));

        // Reject empty, current-directory, and parent-directory names.
        if (string.IsNullOrWhiteSpace(safeName) || safeName is "." or "..")
            throw new ArgumentException("A safe, non-empty name is required.", nameof(name));
        return safeName;
    }

    private static int ParseChunkNumber(string path)
    {
        var name = Path.GetFileName(path);
        if (!name.StartsWith("chunk_", StringComparison.Ordinal)
            || !int.TryParse(name["chunk_".Length..], out var number)
            || number < 0)
            throw new InvalidOperationException("Invalid chunk name.");
        return number;
    }
}