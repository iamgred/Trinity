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
    // Simple endpoint used by humans or monitoring tools to check that the API runs.
    public IActionResult Health() => Ok(new { status = "ok", utc = DateTimeOffset.UtcNow });

    [HttpPost("files/{fileName}/file")]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> ReceiveFile(string fileName, CancellationToken cancellationToken)
    {
        // Enable buffering so ASP.NET Core can safely expose the raw request body.
        Request.EnableBuffering();

        // Start reading at the beginning of the request body.
        Request.Body.Position = 0;

        // Delegate disk writing to the service.
        await _fileTransferService.ReceiveFileAsync(fileName, Request.Body, cancellationToken);

        // Return a JSON confirmation to the C++ caller.
        return Ok(new { message = "File received.", fileName });
    }

    [HttpPost("files/{transferId}/chunks/{chunkNumber:int}")]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> ReceiveChunk(string transferId, int chunkNumber, CancellationToken cancellationToken)
    {
        // The body contains raw bytes, not multipart form data.
        Request.EnableBuffering();
        Request.Body.Position = 0;

        // Store this chunk under its transfer ID and number.
        await _fileTransferService.ReceiveChunkAsync(transferId, chunkNumber, Request.Body, cancellationToken);

        return Ok(new { message = "Chunk received.", transferId, chunkNumber });
    }

    [HttpPost("files/{transferId}/rebuild")]
    public async Task<IActionResult> RebuildFile(string transferId, CancellationToken cancellationToken)
    {
        // Rebuilding is separate so the sender can explicitly signal completion.
        var path = await _fileTransferService.RebuildFileAsync(transferId, cancellationToken);

        return Ok(new { message = "File rebuilt.", transferId, file = path });
    }

    [HttpPost("streams/{streamId}/data")]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> ReceiveStreamData(string streamId, CancellationToken cancellationToken)
    {
        // Each request appends another piece of stream data.
        Request.EnableBuffering();
        Request.Body.Position = 0;

        await _fileTransferService.ReceiveStreamDataAsync(streamId, Request.Body, cancellationToken);

        return Ok(new { message = "Stream data received.", streamId });
    }

    [HttpGet("files")]
    // Return metadata for files directly inside storage/files.
    public async Task<IActionResult> ListFiles() => Ok(await _receivedFileService.ListFilesAsync());

    [HttpGet("files/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        // Ask the service for a safe, read-only file stream.
        var file = await _receivedFileService.OpenFileAsync(fileName);

        // A missing file becomes a standard HTTP 404 response.
        // Otherwise ASP.NET Core streams the file to the downloader.
        return file is null ? NotFound() : File(file, "application/octet-stream", Path.GetFileName(fileName), enableRangeProcessing: true);
    }
}
