using TeamServer.Interface;
using Microsoft.AspNetCore.Mvc;

namespace TeamServer.Controllers;

[ApiController]
[Route("api")]
public sealed class FileTransferController : ControllerBase
{
    // The controller translates HTTP requests into service calls.
    // It should coordinate requests, while the services contain file-transfer logic.
    private readonly IFileTransferService _fileTransferService;
    private readonly IReceivedFileService _receivedFileService;

    public FileTransferController(IFileTransferService fileTransferService, IReceivedFileService receivedFileService)
    {
        _fileTransferService = fileTransferService;
        _receivedFileService = receivedFileService;
    }

    [HttpGet("/api/health")]
    public IActionResult Health() => Ok(new { status = "ok", utc = DateTimeOffset.UtcNow });

    [HttpPost("files/{agentKey}/{fileName}/file")]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> ReceiveFile(string agentKey, string fileName, CancellationToken cancellationToken)
    {
        // Enable buffering so ASP.NET Core can safely expose the raw request body.
        Request.EnableBuffering();

        // Start reading at the beginning of the request body.
        Request.Body.Position = 0;

        // Delegate disk writing to the service.
        await _fileTransferService.ReceiveFileAsync(agentKey, fileName, Request.Body, cancellationToken);

        // Return a JSON confirmation to the C++ caller.
        return Ok(new { message = "File received.", fileName });
    }

    [HttpPost("files/{agentKey}/{transferId}/chunks/{chunkNumber:int}")]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> ReceiveChunk(string agentKey, string transferId, int chunkNumber, CancellationToken cancellationToken)
    {
        // The body contains raw bytes, not multipart form data.
        Request.EnableBuffering();
        Request.Body.Position = 0;

        // Store this chunk under its transfer ID and number.
        await _fileTransferService.ReceiveChunkAsync(agentKey, transferId, chunkNumber, Request.Body, cancellationToken);

        return Ok(new { message = "Chunk received.", transferId, chunkNumber });
    }

    [HttpPost("files/{agentKey}/{transferId}/rebuild")]
    public async Task<IActionResult> RebuildFile(string agentKey, string transferId, CancellationToken cancellationToken)
    {
        // Rebuilding is separate so the sender can explicitly signal completion.
        var path = await _fileTransferService.RebuildFileAsync(agentKey, transferId, cancellationToken);

        return Ok(new { message = "File rebuilt.", transferId, file = path });
    }

    [HttpPost("streams/{agentKey}/{streamId}/data")]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> ReceiveStreamData(string agentKey, string streamId, CancellationToken cancellationToken)
    {
        // Each request appends another piece of stream data.
        Request.EnableBuffering();
        Request.Body.Position = 0;

        await _fileTransferService.ReceiveStreamDataAsync(agentKey, streamId, Request.Body, cancellationToken);

        return Ok(new { message = "Stream data received.", streamId });
    }

    [HttpGet("files")]
    // Return metadata for files directly inside storage/files.
    public async Task<IActionResult> ListFiles([FromQuery] int campaignId) =>
        Ok(await _receivedFileService.ListFilesAsync(campaignId));

    [HttpGet("files/{campaignId:int}/{fileName}")]
    public async Task<IActionResult> DownloadFile(int campaignId, string fileName)
    {
        // Ask the service for a safe, read-only file stream.
        var file = await _receivedFileService.OpenFileAsync(campaignId, fileName);

        // A missing file becomes a standard HTTP 404 response.
        // Otherwise ASP.NET Core streams the file to the downloader.
        return file is null ? NotFound() : File(file, "application/octet-stream", Path.GetFileName(fileName), enableRangeProcessing: true);
    }
}
