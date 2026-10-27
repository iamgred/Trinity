// ================================================================= { START OF FILE } ================================================================= //
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TeamServer.Controllers;
using Trinity.Shared.Configurations;
using Trinity.Shared.DTOs.TeamServer;
using Trinity.Shared.Interfaces;
using Xunit;

namespace TeamServer.Tests.Controllers
{
    public class ServerControllerTests
    {
        private readonly Mock<ILogger<ServerController>> _loggerMock;
        private readonly Mock<IServerNetworkSettings> _settingsMock;

        public ServerControllerTests()
        {
            _loggerMock = new Mock<ILogger<ServerController>>();
            _settingsMock = new Mock<IServerNetworkSettings>();
        }

        [Fact]
        public void GetServerStatus_ReturnsOk()
        {
            // Act
            var controller = CreateController();

            var result = controller.GetServerStatus();

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void GetServerIP_ReturnsOkWithServerIPResponse()
        {
            // Arrange
            const string ipv4 = "192.168.1.100";
            const string ipv6 = "::1";

            _settingsMock
                .Setup(x => x.GetIpV4Address())
                .Returns(ipv4);

            _settingsMock
                .Setup(x => x.GetIpV6Address())
                .Returns(ipv6);

            var controller = CreateController();

            // Act
            var result = controller.GetServerIP();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TeamServerIpResponse>(okResult.Value);

            Assert.Equal(ipv4, response.ipV4);
            Assert.Equal(ipv6, response.ipV6);
        }

        private ServerController CreateController()
        {
            return new ServerController(
                _loggerMock.Object,
                _settingsMock.Object);
        }
    }
}
// ================================================================= { END OF FILE } ================================================================= //