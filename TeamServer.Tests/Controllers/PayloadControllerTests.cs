//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.DTOs.Payload;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;
using Xunit;

namespace TeamServer.Tests.Controllers
{
    public class PayloadControllerTests
    {
        private readonly Mock<IPayloadService> _payloadServiceMock;
        private readonly PayloadController _controller;

        public PayloadControllerTests()
        {
            _payloadServiceMock = new Mock<IPayloadService>();
            _controller = new PayloadController(_payloadServiceMock.Object);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GeneratePayloadAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task GeneratePayloadAsync_WhenSuccessful_ReturnsOk()
        {
            var request = new PayloadCreationDTO
            {
                Name = "TestPayload",
                ListenerID = 1,
                CampaignID = 2,
                Architecture = "x64",
                Type = "WinExe",
                OutputFormat = "exe",
                RetryStrategy = "none",
                isStageless = false
            };

            _payloadServiceMock
                .Setup(x => x.GeneratePayloadAsync(request))
                .ReturnsAsync(Result.Success());

            var result = await _controller.GeneratePayloadAsync(request);

            Assert.IsType<OkResult>(result);

            _payloadServiceMock.Verify(
                x => x.GeneratePayloadAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task GeneratePayloadAsync_WhenServiceFails_ReturnsBadRequest()
        {
            var request = new PayloadCreationDTO
            {
                Name = "TestPayload",
                ListenerID = 1,
                CampaignID = 2,
                Architecture = "x64",
                Type = "WinExe",
                OutputFormat = "exe",
                RetryStrategy = "none",
                isStageless = false
            };

            var error = Trinity.Shared.Errors.Error.Failure(
                "Payload.GenerationFailed",
                "Payload generation failed");

            _payloadServiceMock
                .Setup(x => x.GeneratePayloadAsync(request))
                .ReturnsAsync(error);

            var result = await _controller.GeneratePayloadAsync(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var response = Assert.IsType<Result>(badRequest.Value);

            Assert.False(response.IsSuccess);
            Assert.Equal(error, response.Error);

            _payloadServiceMock.Verify(
                x => x.GeneratePayloadAsync(request),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetPayloadsAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task GetPayloadsAsync_WhenSuccessful_ReturnsOkWithPayloads()
        {
            var payloads = new List<PayloadDTO>
            {
                new PayloadDTO
                {
                    Name = "TestPayload.exe",
                    Size = 1024,
                    Type = "WinExe",
                    CreateTime = DateTime.UtcNow
                }
            };

            _payloadServiceMock
                .Setup(x => x.GetPayloadsAsync())
                .ReturnsAsync(payloads);

            var result = await _controller.GetPayloadsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Same(payloads, okResult.Value);

            _payloadServiceMock.Verify(
                x => x.GetPayloadsAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetPayloadsAsync_WhenServiceFails_ReturnsBadRequest()
        {
            var error = Trinity.Shared.Errors.Error.Failure(
                "Payload.GetPayloads",
                "Failed to retrieve payloads");

            _payloadServiceMock
                .Setup(x => x.GetPayloadsAsync())
                .ReturnsAsync(error);

            var result = await _controller.GetPayloadsAsync();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            var response = Assert.IsType<Result<List<PayloadDTO>>>(badRequest.Value);

            Assert.False(response.IsSuccess);
            Assert.Equal(error, response.Error);

            _payloadServiceMock.Verify(
                x => x.GetPayloadsAsync(),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetPayloadAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task GetPayloadAsync_WhenSuccessful_ReturnsPhysicalFile()
        {
            const int payloadID = 42;
            const string filePath = "/tmp/TestPayload.exe";

            _payloadServiceMock
                .Setup(x => x.GetPayloadFilePathAsync(payloadID))
                .ReturnsAsync(filePath);

            var result = await _controller.GetPayloadAsync(payloadID);

            var physicalFile = Assert.IsType<PhysicalFileResult>(result);

            Assert.Equal(filePath, physicalFile.FileName);
            Assert.Equal("application/octet-stream", physicalFile.ContentType);
            Assert.Equal("TestPayload.exe", physicalFile.FileDownloadName);

            _payloadServiceMock.Verify(
                x => x.GetPayloadFilePathAsync(payloadID),
                Times.Once);
        }

        [Fact]
        public async Task GetPayloadAsync_WhenPayloadNotFound_ReturnsNotFound()
        {
            const int payloadID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Payload.NotFound",
                "Payload was not found");

            _payloadServiceMock
                .Setup(x => x.GetPayloadFilePathAsync(payloadID))
                .ReturnsAsync(error);

            var result = await _controller.GetPayloadAsync(payloadID);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            var response = Assert.IsType<Result<string>>(notFound.Value);

            Assert.False(response.IsSuccess);
            Assert.Equal(error, response.Error);

            _payloadServiceMock.Verify(
                x => x.GetPayloadFilePathAsync(payloadID),
                Times.Once);
        }
    }
}

//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//