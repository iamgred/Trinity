// ======================================================= { START OF FILE } ======================================================= //
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.Enums;
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

        // UploadFile

        [Fact]
        public async Task UploadFile_WhenServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var expectedPath = "test.txt";

            _fileStorageServiceMock
                .Setup(x => x.SaveFileAsync(fileMock.Object))
                .ReturnsAsync(expectedPath);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedPath, okResult.Value);

            _fileStorageServiceMock.Verify(
                x => x.SaveFileAsync(fileMock.Object),
                Times.Once());
        }

        [Fact]
        public async Task UploadFile_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var error = FileError.EmptyFile();

            _fileStorageServiceMock
                .Setup(x => x.SaveFileAsync(fileMock.Object))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.UploadFile(fileMock.Object);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(error, badRequestResult.Value);

            _fileStorageServiceMock.Verify(
                x => x.SaveFileAsync(fileMock.Object),
                Times.Once());
        }

        // DownloadFileAsync

        [Fact]
        public async Task DownloadFileAsync_WhenServiceSucceeds_ReturnsFile()
        {
            // Arrange
            var fileName = "test.txt";
            var folderName = FileFolders.Uploads;
            var fileBytes = new byte[] { 1, 2, 3, 4 };

            _fileStorageServiceMock
                .Setup(x => x.GetFileAsync(fileName, folderName))
                .ReturnsAsync(fileBytes);

            // Act
            var result = await _controller.DownloadFileAsync(fileName, folderName);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal(fileBytes, fileResult.FileContents);
            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);

            _fileStorageServiceMock.Verify(
                x => x.GetFileAsync(fileName, folderName),
                Times.Once());
        }

        [Fact]
        public async Task DownloadFileAsync_WhenServiceFails_ReturnsNotFound()
        {
            // Arrange
            var fileName = "missing.txt";
            var folderName = FileFolders.Uploads;
            var error = FileError.FileNotFound(fileName);

            _fileStorageServiceMock
                .Setup(x => x.GetFileAsync(fileName, folderName))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.DownloadFileAsync(fileName, folderName);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFoundResult.Value);

            _fileStorageServiceMock.Verify(
                x => x.GetFileAsync(fileName, folderName),
                Times.Once());
        }

        // List Files

        [Fact]
        public void ListFiles_WhenServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var folderName = FileFolders.Uploads;

            var expectedFiles = new List<string>
            {
                "test.txt",
                "document.pdf",
                "image.png"
            };

            _fileStorageServiceMock
                .Setup(x => x.ListFiles(folderName))
                .Returns(expectedFiles);

            // Act
            var result = _controller.ListFiles(folderName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedFiles, okResult.Value);

            _fileStorageServiceMock.Verify(
                x => x.ListFiles(folderName),
                Times.Once());
        }

        [Fact]
        public void ListFiles_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var folderName = FileFolders.Uploads;
            var error = FileError.FileNotFound("No Files Found.");

            _fileStorageServiceMock
                .Setup(x => x.ListFiles(folderName))
                .Returns(error);

            // Act
            var result = _controller.ListFiles(folderName);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _fileStorageServiceMock.Verify(
                x => x.ListFiles(folderName),
                Times.Once());
        }
    }
}
// ======================================================= { END OF FILE } ======================================================= //
