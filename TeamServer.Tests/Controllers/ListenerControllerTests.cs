//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { START OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //

using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.DTOs.Listener;
using Trinity.Shared.DTOs.Listener.Http;
using Trinity.Shared.DTOs.Listener.Smb;
using Trinity.Shared.DTOs.Listener.Tcp;
using Trinity.Shared.Errors;
using Trinity.Shared.Results;
using Xunit;

namespace TeamServer.Tests.Controllers
{
    public class ListenerControllerTests
    {
        private readonly Mock<IListenerService> _listenerServiceMock;
        private readonly ListenerController _controller;

        public ListenerControllerTests()
        {
            _listenerServiceMock = new Mock<IListenerService>();
            _controller = new ListenerController(_listenerServiceMock.Object);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetHttpListeners
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task GetHttpListeners_WhenSuccessful_ReturnsOkWithListeners()
        {
            var listeners = new List<ListenerResponse>
            {
                new ListenerResponse(1, "Test HTTP", "HTTP"),
                new ListenerResponse(2, "Test TCP", "TCP")
            };

            _listenerServiceMock
                .Setup(x => x.GetListenersAsync())
                .ReturnsAsync(listeners);

            var result = await _controller.GetHttpListeners();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Same(listeners, okResult.Value);

            _listenerServiceMock.Verify(
                x => x.GetListenersAsync(),
                Times.Once);
        }

        [Fact]
        public async Task GetHttpListeners_WhenServiceFails_ReturnsOkWithNullResponse()
        {
            var error = Trinity.Shared.Errors.Error.Failure(
                "Listener.GetListeners",
                "Failed to retrieve listeners");

            _listenerServiceMock
                .Setup(x => x.GetListenersAsync())
                .ReturnsAsync(error);

            var result = await _controller.GetHttpListeners();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Null(okResult.Value);

            _listenerServiceMock.Verify(
                x => x.GetListenersAsync(),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // RestartListenerAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task RestartListenerAsync_WhenSuccessful_ReturnsOk()
        {
            const int listenerID = 42;

            _listenerServiceMock
                .Setup(x => x.RestartListenerAsync(listenerID))
                .ReturnsAsync(Result.Success());

            var result = await _controller.RestartListenerAsync(listenerID);

            Assert.IsType<OkResult>(result);

            _listenerServiceMock.Verify(
                x => x.RestartListenerAsync(listenerID),
                Times.Once);
        }

        [Fact]
        public async Task RestartListenerAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            const int listenerID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Listener.NotFound",
                "Listener was not found");

            _listenerServiceMock
                .Setup(x => x.RestartListenerAsync(listenerID))
                .ReturnsAsync(error);

            var result = await _controller.RestartListenerAsync(listenerID);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _listenerServiceMock.Verify(
                x => x.RestartListenerAsync(listenerID),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // StartHttpListener
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task StartHttpListener_WhenSuccessful_ReturnsCreated()
        {
            var config = new HttpConfig(
                8080,
                8081,
                "TestAgent",
                "Content-type: application/json",
                new List<string> { "localhost" },
                "none",
                "none");

            var request = new CreateHttpListenerRequest(
                "Test HTTP Listener",
                config);

            _listenerServiceMock
                .Setup(x => x.CreateHttpListenerAsync(request))
                .ReturnsAsync(Result.Success());

            var result = await _controller.StartHttpListener(request);

            Assert.IsType<CreatedResult>(result);

            _listenerServiceMock.Verify(
                x => x.CreateHttpListenerAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task StartHttpListener_WhenServiceFails_ReturnsBadRequestWithError()
        {
            var config = new HttpConfig(
                8080,
                8081,
                "TestAgent",
                "Content-type: application/json",
                new List<string> { "localhost" },
                "none",
                "none");

            var request = new CreateHttpListenerRequest(
                "Test HTTP Listener",
                config);

            var error = Trinity.Shared.Errors.Error.Failure(
                "Listener.CreateHttp",
                "Failed to create HTTP listener");

            _listenerServiceMock
                .Setup(x => x.CreateHttpListenerAsync(request))
                .ReturnsAsync(error);

            var result = await _controller.StartHttpListener(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _listenerServiceMock.Verify(
                x => x.CreateHttpListenerAsync(request),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // CreateTcpListenerAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task CreateTcpListenerAsync_WhenSuccessful_ReturnsCreated()
        {
            var config = new TcpConfig(8082);

            var request = new CreateTcpListenerRequest(
                "Test TCP Listener",
                config);

            _listenerServiceMock
                .Setup(x => x.CreateTCPListenerAsync(request))
                .ReturnsAsync(Result.Success());

            var result = await _controller.CreateTcpListenerAsync(request);

            Assert.IsType<CreatedResult>(result);

            _listenerServiceMock.Verify(
                x => x.CreateTCPListenerAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task CreateTcpListenerAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            var config = new TcpConfig(8082);

            var request = new CreateTcpListenerRequest(
                "Test TCP Listener",
                config);

            var error = Trinity.Shared.Errors.Error.Failure(
                "Listener.CreateTcp",
                "Failed to create TCP listener");

            _listenerServiceMock
                .Setup(x => x.CreateTCPListenerAsync(request))
                .ReturnsAsync(error);

            var result = await _controller.CreateTcpListenerAsync(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _listenerServiceMock.Verify(
                x => x.CreateTCPListenerAsync(request),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // CreateSmbListenerAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task CreateSmbListenerAsync_WhenSuccessful_ReturnsCreated()
        {
            var config = new SmbConfig("test-pipe");

            var request = new CreateSmbRequest(
                "Test SMB Listener",
                config);

            _listenerServiceMock
                .Setup(x => x.CreateSmbListenerAsync(request))
                .ReturnsAsync(Result.Success());

            var result = await _controller.CreateSmbListenerAsync(request);

            Assert.IsType<CreatedResult>(result);

            _listenerServiceMock.Verify(
                x => x.CreateSmbListenerAsync(request),
                Times.Once);
        }

        [Fact]
        public async Task CreateSmbListenerAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            var config = new SmbConfig("test-pipe");

            var request = new CreateSmbRequest(
                "Test SMB Listener",
                config);

            var error = Trinity.Shared.Errors.Error.Failure(
                "Listener.CreateSmb",
                "Failed to create SMB listener");

            _listenerServiceMock
                .Setup(x => x.CreateSmbListenerAsync(request))
                .ReturnsAsync(error);

            var result = await _controller.CreateSmbListenerAsync(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _listenerServiceMock.Verify(
                x => x.CreateSmbListenerAsync(request),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetListenerByIDAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task GetListenerByIDAsync_WhenSuccessful_ReturnsOkWithListener()
        {
            const int listenerID = 42;

            var config = System.Text.Json.JsonDocument.Parse(
                """{"port":8080}""");

            var response = new GetListenerDetailsResponse(
                listenerID,
                "Test Listener",
                config);

            _listenerServiceMock
                .Setup(x => x.GetListener(listenerID))
                .ReturnsAsync(response);

            var result = await _controller.GetListenerByIDAsync(listenerID);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Same(response, okResult.Value);

            _listenerServiceMock.Verify(
                x => x.GetListener(listenerID),
                Times.Once);
        }

        [Fact]
        public async Task GetListenerByIDAsync_WhenListenerNotFound_ReturnsNotFoundWithError()
        {
            const int listenerID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Listener.NotFound",
                "Listener was not found");

            _listenerServiceMock
                .Setup(x => x.GetListener(listenerID))
                .ReturnsAsync(error);

            var result = await _controller.GetListenerByIDAsync(listenerID);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFound.Value);

            _listenerServiceMock.Verify(
                x => x.GetListener(listenerID),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // UpdateHttpListenerAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task UpdateHttpListenerAsync_WhenSuccessful_ReturnsOk()
        {
            const int listenerID = 42;

            var config = new HttpConfig(
                8080,
                8081,
                "TestAgent",
                "Content-type: application/json",
                new List<string> { "localhost" },
                "none",
                "none");

            var request = new UpdateHttpListenerRequest(config);

            _listenerServiceMock
                .Setup(x => x.UpdateHttpListenerAsync(listenerID, config))
                .ReturnsAsync(Result.Success());

            var result = await _controller.UpdateHttpListenerAsync(
                listenerID,
                request);

            Assert.IsType<OkResult>(result);

            _listenerServiceMock.Verify(
                x => x.UpdateHttpListenerAsync(listenerID, config),
                Times.Once);
        }

        [Fact]
        public async Task UpdateHttpListenerAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            const int listenerID = 42;

            var config = new HttpConfig(
                8080,
                8081,
                "TestAgent",
                "Content-type: application/json",
                new List<string> { "localhost" },
                "none",
                "none");

            var request = new UpdateHttpListenerRequest(config);

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Listener.NotFound",
                "Listener was not found");

            _listenerServiceMock
                .Setup(x => x.UpdateHttpListenerAsync(listenerID, config))
                .ReturnsAsync(error);

            var result = await _controller.UpdateHttpListenerAsync(
                listenerID,
                request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _listenerServiceMock.Verify(
                x => x.UpdateHttpListenerAsync(listenerID, config),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // DeleteListenerAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async Task DeleteListenerAsync_WhenSuccessful_ReturnsOk()
        {
            const int listenerID = 42;

            _listenerServiceMock
                .Setup(x => x.RemoveListenerAsync(listenerID))
                .ReturnsAsync(Result.Success());

            var result = await _controller.DeleteListenerAsync(listenerID);

            Assert.IsType<OkResult>(result);

            _listenerServiceMock.Verify(
                x => x.RemoveListenerAsync(listenerID),
                Times.Once);
        }

        [Fact]
        public async Task DeleteListenerAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            const int listenerID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Listener.NotFound",
                "Listener was not found");

            _listenerServiceMock
                .Setup(x => x.RemoveListenerAsync(listenerID))
                .ReturnsAsync(error);

            var result = await _controller.DeleteListenerAsync(listenerID);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _listenerServiceMock.Verify(
                x => x.RemoveListenerAsync(listenerID),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ { END OF FILE } ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ //
    }
}