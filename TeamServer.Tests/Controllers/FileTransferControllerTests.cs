using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;

namespace TeamServer.Tests.Controllers;

public sealed class FileTransferControllerTests
{
    private readonly Mock<IFileTransferService> _fileTransferService = new();
    private readonly Mock<IReceivedFileService> _receivedFileService = new();
    private readonly FileTransferController _controller;

    public FileTransferControllerTests()
    {
        _controller = new FileTransferController(
            _fileTransferService.Object,
            _receivedFileService.Object);
    }

    [Fact]
    public void Health_ReturnsOk()
    {
        var result = _controller.Health();

        var response = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("ok", response.Value!.GetType().GetProperty("status")!.GetValue(response.Value));
    }

    [Fact]
    public async Task ReceiveFile_PassesRawRequestBodyToService()
    {
        var body = new byte[] { 1, 2, 3 };
        SetRequestBody(body);

        var result = await _controller.ReceiveFile("agent", "transfer", "sample.bin", 12, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        _fileTransferService.Verify(x => x.ReceiveFileAsync(
            "agent",
            "transfer",
            "sample.bin",
            12,
            It.Is<Stream>(stream => stream.ReadByte() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveChunk_PassesChunkNumberAndBodyToService()
    {
        SetRequestBody(new byte[] { 4, 5 });

        var result = await _controller.ReceiveChunk("agent", "transfer", 3, 12, CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        _fileTransferService.Verify(x => x.ReceiveChunkAsync(
            "agent",
            "transfer",
            12,
            3,
            It.Is<Stream>(stream => stream.ReadByte() == 4),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RebuildFile_ReturnsRebuiltPath()
    {
        _fileTransferService
            .Setup(x => x.RebuildFileAsync("agent", "transfer", 12, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@"storage\files\7\transfer_rebuilt");

        var result = await _controller.RebuildFile("agent", "transfer", 12, 5, CancellationToken.None);

        var response = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("transfer_rebuilt", response.Value!.ToString());
    }

    [Fact]
    public async Task ReceiveStreamData_PassesRawRequestBodyToService()
    {
        SetRequestBody(new byte[] { 9, 8 });

        var result = await _controller.ReceiveStreamData("agent", "stream", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
        _fileTransferService.Verify(x => x.ReceiveStreamDataAsync(
            "agent",
            "stream",
            It.Is<Stream>(stream => stream.ReadByte() == 9),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadFile_ReturnsServiceFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(path, new byte[] { 7, 6 });
            await using var expected = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            _receivedFileService
                .Setup(x => x.OpenFileAsync("agent", 7, "sample.bin"))
                .ReturnsAsync(expected);

            var result = await _controller.DownloadFile(7, "sample.bin", "agent");

            var file = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/octet-stream", file.ContentType);
            Assert.Equal("sample.bin", file.FileDownloadName);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private void SetRequestBody(byte[] body)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request =
                {
                    Body = new MemoryStream(body)
                }
            }
        };
    }
}
