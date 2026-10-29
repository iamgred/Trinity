using TeamServer.Interface;
using TeamServer.Repositories;

namespace TeamServer.Services;

public sealed class ReceivedFileService : IReceivedFileService
{
    // This service lists completed/top-level files, not chunk directories.
    private readonly string _filesDirectory;
    private readonly AgentRepository _agentRepository;

    public ReceivedFileService(IConfiguration configuration, IWebHostEnvironment environment, AgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
        // Resolve the same configured storage location used by FileTransferService.
        var root = Path.GetFullPath(configuration["StoragePath"] ?? Path.Combine(environment.ContentRootPath, "storage"));
        _filesDirectory = Path.Combine(root, "files");
        Directory.CreateDirectory(_filesDirectory);
    }

    public async Task<IReadOnlyList<ReceivedFileInfo>> ListFilesAsync(string agentKey, int campaignId)
    {
        var agent = await _agentRepository.GetAgentByCheckInUUID(agentKey);
        if (agent is null || agent.CampaignID != campaignId)
            return [];
        // Enumerate only direct files.
        // Chunk directories are intentionally excluded from the public list.
        var campaignDirectory = Path.Combine(_filesDirectory, campaignId.ToString());
        if (!Directory.Exists(campaignDirectory))
            return [];

        var files = Directory.EnumerateFiles(campaignDirectory)
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.Name, StringComparer.Ordinal)
            .Select(file => new ReceivedFileInfo(file.Name, file.Length, file.LastWriteTimeUtc))
            .ToArray();
        return files;
    }

    public async Task<FileStream?> OpenFileAsync(string agentKey, int campaignId, string fileName)
    {
        var agent = await _agentRepository.GetAgentByCheckInUUID(agentKey);
        if (agent is null || agent.CampaignID != campaignId)
            return null;
        // Strip directory components before combining with the storage path.
        // This prevents a download request from navigating outside storage.
        var safeName = Path.GetFileName(Uri.UnescapeDataString(fileName));
        if (string.IsNullOrWhiteSpace(safeName) || safeName is "." or "..")
            return null;

        // Return null so the controller can produce a normal HTTP 404 response.
        var path = Path.Combine(_filesDirectory, campaignId.ToString(), safeName);
        if (!File.Exists(path))
            return null;

        // Open read-only and allow other readers to access the same file.
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
