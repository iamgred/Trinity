//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.DTOs.Checkin;
using Trinity.Shared.DTOs.Task;
using Trinity.Shared.DTOs.Agent;
using AsyncTask = System.Threading.Tasks.Task;
using Trinity.Shared.Enums;

namespace TeamServer.Tests.Controllers
{
    public class AgentControllerTests
    {
        private readonly Mock<IAgentService> _agentServiceMock;
        private readonly AgentController _controller;

        public AgentControllerTests()
        {
            _agentServiceMock = new Mock<IAgentService>();
            _controller = new AgentController(_agentServiceMock.Object);
        }

        // =======================================================
        // GetAgents
        // =======================================================

        [Fact]
        public async AsyncTask GetAgents_Success_ReturnsOk()
        {
            var agents = new List<AgentResponse>();

            _agentServiceMock
                .Setup(service => service.GetAgentsAsync())
                .ReturnsAsync(agents);

            var result = await _controller.GetAgents();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(agents, okResult.Value);

            _agentServiceMock.Verify(
                service => service.GetAgentsAsync(),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetAgents_Failure_ReturnsBadRequest()
        {
            var error = Trinity.Shared.Errors.Error.Failure(
                "Agent.GetAgents",
                "Failed to retrieve agents");

            _agentServiceMock
                .Setup(service => service.GetAgentsAsync())
                .ReturnsAsync(error);

            var result = await _controller.GetAgents();

            Assert.IsType<BadRequestResult>(result);

            _agentServiceMock.Verify(
                service => service.GetAgentsAsync(),
                Times.Once);
        }

        // =======================================================
        // Checkin
        // =======================================================

        [Fact]
        public async AsyncTask Checkin_Success_ReadsBodyAndReturnsOk()
        {
            const string blob = "VGhpcyBpcyBhIHRlc3QgYmxvYg==";
            const string response = "response";

            _agentServiceMock
                .Setup(service => service.Checkin(blob))
                .ReturnsAsync(response);

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Body = new MemoryStream(
                Encoding.UTF8.GetBytes(blob));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await _controller.Checkin();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);

            _agentServiceMock.Verify(
                service => service.Checkin(blob),
                Times.Once);
        }

        [Fact]
        public async AsyncTask Checkin_Failure_ReturnsBadRequest()
        {
            const string blob = "VGhpcyBpcyBhIHRlc3QgYmxvYg==";

            var error = Trinity.Shared.Errors.Error.Failure(
                "Agent.Checkin",
                "Agent check-in failed");

            _agentServiceMock
                .Setup(service => service.Checkin(blob))
                .ReturnsAsync(error);

            var httpContext = new DefaultHttpContext();

            httpContext.Request.Body = new MemoryStream(
                Encoding.UTF8.GetBytes(blob));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var result = await _controller.Checkin();

            Assert.IsType<BadRequestResult>(result);

            _agentServiceMock.Verify(
                service => service.Checkin(blob),
                Times.Once);
        }

        // =======================================================
        // GetIntialBase64
        // =======================================================

        [Fact]
        public void GetIntialBase64_ReturnsOkWithEncodedRequest()
        {
            var request = new IntialCheckInRequest(
                "192.168.1.10",
                "8.8.8.8",
                "Windows",
                "testuser",
                "test.exe",
                1234,
                "Medium",
                "AA:BB:CC:DD:EE:FF",
                "Test Motherboard",
                16384,
                500.0,
                250.0,
                8);

            const string uuid = "test-uuid";

            var result = _controller.GetIntialBase64(request, uuid);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var encoded = Assert.IsType<string>(okResult.Value);

            var decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(encoded));

            var expectedUuid = JsonSerializer.Serialize(uuid);
            var expectedRequest = JsonSerializer.Serialize(request);

            Assert.StartsWith(
                uuid,
                decoded);

            Assert.Contains(
                "\"internalIP\":\"192.168.1.10\"",
                decoded);

            Assert.Contains(
                "\"externalIP\":\"8.8.8.8\"",
                decoded);

            Assert.Contains(
                "\"OS\":\"Windows\"",
                decoded);

            Assert.Contains(
                "\"user\":\"testuser\"",
                decoded);
        }

        // =======================================================
        // GetFullBase64
        // =======================================================

        [Fact]
        public void GetFullBase64_ReturnsOkWithEncodedRequest()
        {
            var request = new List<TaskResp<string>>
            {
                new TaskResp<string>
                {
                    TaskID = 1,
                    TaskType = 2,
                    TaskCode = 3,
                    Status = 0,
                    Data = "test response"
                }
            };

            const string uuid = "test-uuid";

            var result = _controller.GetFullBase64(request, uuid);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var encoded = Assert.IsType<string>(okResult.Value);

            var decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(encoded));

            Assert.StartsWith(uuid, decoded);
            Assert.Contains("\"TaskID\":1", decoded);
            Assert.Contains("\"TaskType\":2", decoded);
            Assert.Contains("\"TaskCode\":3", decoded);
            Assert.Contains("\"Status\":0", decoded);
            Assert.Contains("\"Data\":\"test response\"", decoded);
        }

        // =======================================================
        // GetFullBase64Error
        // =======================================================

        [Fact]
        public void GetFullBase64Error_ReturnsOkWithEncodedRequest()
        {
            var request = new List<TaskResp<Trinity.Shared.DTOs.Task.Error>>
            {
                new TaskResp<Trinity.Shared.DTOs.Task.Error>
                {
                    TaskID = 1,
                    TaskType = 2,
                    TaskCode = 3,
                    Status = 1,
                    Data = null!
                }
            };

            const string uuid = "test-uuid";

            var result = _controller.GetFullBase64Error(request, uuid);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var encoded = Assert.IsType<string>(okResult.Value);

            var decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(encoded));

            Assert.StartsWith(uuid, decoded);
            Assert.Contains("\"TaskID\":1", decoded);
            Assert.Contains("\"TaskType\":2", decoded);
            Assert.Contains("\"TaskCode\":3", decoded);
            Assert.Contains("\"Status\":1", decoded);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//