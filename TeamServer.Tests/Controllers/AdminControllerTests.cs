// ===================================================== { START OF FILE } ===================================================== //

using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.Models;
using Trinity.Shared.Results;
using Xunit;
using AsyncTask = System.Threading.Tasks.Task;

namespace TeamServer.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _adminServiceMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _adminServiceMock = new Mock<IAdminService>();
            _controller = new AdminController(_adminServiceMock.Object);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetAdminsAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async AsyncTask GetAdminsAsync_WhenSuccessful_ReturnsOkWithAdmins()
        {
            var admins = new List<Admin>
            {
                new Admin
                {
                    ID = 1,
                    Username = "admin1",
                    Password = "password1",
                    LastLogin = DateTime.UtcNow
                },
                new Admin
                {
                    ID = 2,
                    Username = "admin2",
                    Password = "password2",
                    LastLogin = DateTime.UtcNow
                }
            };

            _adminServiceMock
                .Setup(x => x.GetAdminsAsync())
                .ReturnsAsync(admins);

            var result = await _controller.GetAdminsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Same(admins, okResult.Value);

            _adminServiceMock.Verify(
                x => x.GetAdminsAsync(),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetAdminsAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            var error = Trinity.Shared.Errors.Error.Failure(
                "Admin.GetAdmins",
                "Failed to retrieve admins");

            _adminServiceMock
                .Setup(x => x.GetAdminsAsync())
                .ReturnsAsync(error);

            var result = await _controller.GetAdminsAsync();

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _adminServiceMock.Verify(
                x => x.GetAdminsAsync(),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // GetAdminByIdAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async AsyncTask GetAdminByIdAsync_WhenSuccessful_ReturnsOkWithAdmin()
        {
            const int adminID = 42;

            var admin = new Admin
            {
                ID = adminID,
                Username = "testadmin",
                Password = "testpassword",
                LastLogin = DateTime.UtcNow
            };

            _adminServiceMock
                .Setup(x => x.GetAdminByIdAsync(adminID))
                .ReturnsAsync(admin);

            var result = await _controller.GetAdminByIdAsync(adminID);

            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Same(admin, okResult.Value);

            _adminServiceMock.Verify(
                x => x.GetAdminByIdAsync(adminID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetAdminByIdAsync_WhenAdminNotFound_ReturnsNotFound()
        {
            const int adminID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Admin.NotFound",
                "Admin was not found");

            _adminServiceMock
                .Setup(x => x.GetAdminByIdAsync(adminID))
                .ReturnsAsync(error);

            var result = await _controller.GetAdminByIdAsync(adminID);

            Assert.IsType<NotFoundResult>(result);

            _adminServiceMock.Verify(
                x => x.GetAdminByIdAsync(adminID),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // CreateAdminAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async AsyncTask CreateAdminAsync_WhenSuccessful_ReturnsOk()
        {
            var admin = new Admin
            {
                ID = 1,
                Username = "newadmin",
                Password = "password123",
                LastLogin = DateTime.UtcNow
            };

            _adminServiceMock
                .Setup(x => x.CreateAdminAsync(admin))
                .ReturnsAsync(Result.Success());

            var result = await _controller.CreateAdminAsync(admin);

            Assert.IsType<OkResult>(result);

            _adminServiceMock.Verify(
                x => x.CreateAdminAsync(admin),
                Times.Once);
        }

        [Fact]
        public async AsyncTask CreateAdminAsync_WhenServiceFails_ReturnsBadRequestWithError()
        {
            var admin = new Admin
            {
                ID = 1,
                Username = "newadmin",
                Password = "password123",
                LastLogin = DateTime.UtcNow
            };

            var error = Trinity.Shared.Errors.Error.Failure(
                "Admin.Create",
                "Failed to create admin");

            _adminServiceMock
                .Setup(x => x.CreateAdminAsync(admin))
                .ReturnsAsync(error);

            var result = await _controller.CreateAdminAsync(admin);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequest.Value);

            _adminServiceMock.Verify(
                x => x.CreateAdminAsync(admin),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // UpdateAdminAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async AsyncTask UpdateAdminAsync_WhenSuccessful_SetsRouteIdAndReturnsOk()
        {
            const int adminID = 42;

            var admin = new Admin
            {
                ID = 999,
                Username = "updatedadmin",
                Password = "newpassword",
                LastLogin = DateTime.UtcNow
            };

            _adminServiceMock
                .Setup(x => x.UpdateAdminAsync(admin))
                .ReturnsAsync(Result.Success());

            var result = await _controller.UpdateAdminAsync(adminID, admin);

            Assert.IsType<OkResult>(result);

            Assert.Equal(adminID, admin.ID);

            _adminServiceMock.Verify(
                x => x.UpdateAdminAsync(admin),
                Times.Once);
        }

        [Fact]
        public async AsyncTask UpdateAdminAsync_WhenAdminNotFound_SetsRouteIdAndReturnsNotFoundWithError()
        {
            const int adminID = 42;

            var admin = new Admin
            {
                ID = 999,
                Username = "updatedadmin",
                Password = "newpassword",
                LastLogin = DateTime.UtcNow
            };

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Admin.NotFound",
                "Admin was not found");

            _adminServiceMock
                .Setup(x => x.UpdateAdminAsync(admin))
                .ReturnsAsync(error);

            var result = await _controller.UpdateAdminAsync(adminID, admin);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFound.Value);
            Assert.Equal(adminID, admin.ID);

            _adminServiceMock.Verify(
                x => x.UpdateAdminAsync(admin),
                Times.Once);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        // DeleteAdminAsync
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//

        [Fact]
        public async AsyncTask DeleteAdminAsync_WhenSuccessful_ReturnsOk()
        {
            const int adminID = 42;

            _adminServiceMock
                .Setup(x => x.DeleteAdminAsync(adminID))
                .ReturnsAsync(Result.Success());

            var result = await _controller.DeleteAdminAsync(adminID);

            Assert.IsType<OkResult>(result);

            _adminServiceMock.Verify(
                x => x.DeleteAdminAsync(adminID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask DeleteAdminAsync_WhenAdminNotFound_ReturnsNotFoundWithError()
        {
            const int adminID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Admin.NotFound",
                "Admin was not found");

            _adminServiceMock
                .Setup(x => x.DeleteAdminAsync(adminID))
                .ReturnsAsync(error);

            var result = await _controller.DeleteAdminAsync(adminID);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFound.Value);

            _adminServiceMock.Verify(
                x => x.DeleteAdminAsync(adminID),
                Times.Once);
        }
    }
}

// ===================================================== { END OF FILE } ===================================================== //