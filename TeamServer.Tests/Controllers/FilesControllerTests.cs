// ======================================================= { START OF FILE } ======================================================= //
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.Errors;

// https://docs.educationsmediagroup.com/unit-testing-csharp/moq/verifications
namespace TeamServer.Tests.Controllers
{
    public class FilesControllerTests
    {
        private readonly Mock<IFileStorageService> _fileStorageServiceMock;
        private readonly FilesController _controller;

        public FilesControllerTests()
        {
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _controller = new FilesController(_fileStorageServiceMock.Object);
        }

        // UploadFileAsync

        [Fact]
        public async Task UploadFileAsync_WhenServiceSucceeds_ReturnsOk()
        {
            // Arange
            var fileMock = new Mock<IFormFile>();
            var expectedPath = "uploads/test.txt";

            _fileStorageServiceMock.Setup(x => x.SaveFileAsync(fileMock.Object)).ReturnsAsync(expectedPath);

            // Act
            var result = await _controller.UploadFileAsync(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedPath, okResult.Value);

            _fileStorageServiceMock.Verify(x => x.SaveFileAsync(fileMock.Object), Times.Once());
        }

        [Fact]
        public async Task UploadFileAsync_WhenServiceFails_ReturnsBadRequest()
        {
            // Arange
            var fileMock = new Mock<IFormFile>();
            var error = FileError.EmptyFile();

            _fileStorageServiceMock.Setup(x => x.SaveFileAsync(fileMock.Object)).ReturnsAsync(error);

            // Act
            var result = await _controller.UploadFileAsync(fileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequestResult.Value);

            _fileStorageServiceMock.Verify(x => x.SaveFileAsync(fileMock.Object), Times.Once());
        }

        // DownloadFileAsync
        [Fact]
        public async Task DownloadFileAsync_WhenServiceSucceeds_ReturnsFile()
        {
            // Arrange
            var fileName = "test.txt";
            var fileBytes = new byte[] { 1, 2, 3, 4 };

            _fileStorageServiceMock.Setup(x => x.GetFileAsync(fileName)).ReturnsAsync(fileBytes);

            // Act
            var result = await _controller.DownloadFileAsync(fileName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal(fileBytes, fileResult.FileContents);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);

            _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileName), Times.Once());
        }

        [Fact]
        public async Task DownloadFileAsync_WhenServiceFails_ReturnsNotFound()
        {
            // Arrange
            var fileName = "missing.txt";
            var error = FileError.FileNotFound(fileName);

            _fileStorageServiceMock.Setup(x => x.GetFileAsync(fileName)).ReturnsAsync(error);

            // Act 
            var result = await _controller.DownloadFileAsync(fileName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFoundResult.Value);

            _fileStorageServiceMock.Verify(x => x.GetFileAsync(fileName), Times.Once());
        }

        // List Files
        [Fact]
        public void ListFiles_WhenServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var expectedFiles = new List<string>
            {
                "test.txt",
                "document.pdf",
                "image.png"
            };

            _fileStorageServiceMock.Setup(x => x.ListFiles()).Returns(expectedFiles);

            // Act
            var result = _controller.ListFiles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedFiles, okResult.Value);

            _fileStorageServiceMock.Verify(x => x.ListFiles(), Times.Once());
        }

        [Fact]
        public void ListFiles_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var error = FileError.FileNotFound("No Files Found.");

            _fileStorageServiceMock.Setup(x => x.ListFiles()).Returns(error);

            // Act
            var result = _controller.ListFiles();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _fileStorageServiceMock.Verify(x => x.ListFiles(), Times.Once());
        }
    }
}
// ======================================================= { END OF FILE } ======================================================= //