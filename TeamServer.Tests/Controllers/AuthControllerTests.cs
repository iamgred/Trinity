// ================================================================= { START OF FILE } ================================================================= //
using Microsoft.AspNetCore.Mvc;
using TeamServer.Controllers;
using Trinity.Shared.DTOs.Authentication;
using Xunit;

namespace TeamServer.Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public void Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            var controller = new AuthController();

            var loginDTO = new LoginDTO
            {
                Username = "testOperator",
                Password = "testOperator123"
            };

            // Act
            var result = controller.Login(loginDTO);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Login_WithInvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var controller = new AuthController();

            var loginDTO = new LoginDTO
            {
                Username = "wrongUsername",
                Password = "wrongPassword"
            };

            // Act
            var result = controller.Login(loginDTO);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }
    }
}
// ================================================================= { END OF FILE } ================================================================= //