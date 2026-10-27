// ======================================================= { START OF FILE } ======================================================= //
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.Errors;
using Trinity.Shared.Models;
using Trinity.Shared.Results;
using AsyncTask = System.Threading.Tasks.Task;

namespace TeamServer.Tests.Controllers
{
    public class OperatorControllerTests
    {
        private readonly Mock<IOperatorService> _operatorServiceMock;
        private readonly Mock<IValidator<Operator>> _operatorValidatorMock;
        private readonly OperatorController _controller;

        public OperatorControllerTests()
        {
            _operatorServiceMock = new Mock<IOperatorService>();
            _operatorValidatorMock = new Mock<IValidator<Operator>>();

            _controller = new OperatorController(_operatorServiceMock.Object, _operatorValidatorMock.Object);
        }

        // =======================================================
        // GetOperatorsAsync
        // =======================================================

        [Fact]
        public async AsyncTask GetOperatorsAsync_WhenServiceSucceeds_ReturnsOk()
        {
            // Arrange
            var expectedOperators = new List<Operator>
            {
                new Operator
                {
                    ID = 1,
                    Username = "operator1",
                    Password = "Password123!"
                },
                new Operator
                {
                    ID = 2,
                    Username = "operator2",
                    Password = "Password123!"
                }
            };

            _operatorServiceMock
                .Setup(x => x.GetOperatorsAsync())
                .ReturnsAsync(expectedOperators);

            // Act
            var result = await _controller.GetOperatorsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedOperators, okResult.Value);

            _operatorServiceMock.Verify(
                x => x.GetOperatorsAsync(),
                Times.Once());
        }

        [Fact]
        public async AsyncTask GetOperatorsAsync_WhenServiceReturnsEmptyList_ReturnsOk()
        {
            // Arrange
            var expectedOperators = new List<Operator>();

            _operatorServiceMock
                .Setup(x => x.GetOperatorsAsync())
                .ReturnsAsync(expectedOperators);

            // Act
            var result = await _controller.GetOperatorsAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedOperators, okResult.Value);

            _operatorServiceMock.Verify(
                x => x.GetOperatorsAsync(),
                Times.Once());
        }

        [Fact]
        public async AsyncTask GetOperatorsAsync_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var error = OperatorError.NotFound(0);

            _operatorServiceMock
                .Setup(x => x.GetOperatorsAsync())
                .ReturnsAsync(error);

            // Act
            var result = await _controller.GetOperatorsAsync();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _operatorServiceMock.Verify(
                x => x.GetOperatorsAsync(),
                Times.Once());
        }

        // =======================================================
        // GetOperatorByIdAsync
        // =======================================================

        [Fact]
        public async AsyncTask GetOperatorByIdAsync_WhenOperatorExists_ReturnsOk()
        {
            // Arrange
            var operatorID = 1;
            var expectedOperator = new Operator
            {
                ID = operatorID,
                Username = "operator1",
                Password = "Password123!"
            };

            _operatorServiceMock
                .Setup(x => x.GetOperatorByIdAsync(operatorID))
                .ReturnsAsync(expectedOperator);

            // Act
            var result = await _controller.GetOperatorByIdAsync(operatorID);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.Equal(expectedOperator, okResult.Value);

            _operatorServiceMock.Verify(
                x => x.GetOperatorByIdAsync(operatorID),
                Times.Once());
        }

        [Fact]
        public async AsyncTask GetOperatorByIdAsync_WhenOperatorDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var operatorID = 999;
            var error = OperatorError.NotFound(operatorID);

            _operatorServiceMock
                .Setup(x => x.GetOperatorByIdAsync(operatorID))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.GetOperatorByIdAsync(operatorID);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFoundResult.Value);

            _operatorServiceMock.Verify(
                x => x.GetOperatorByIdAsync(operatorID),
                Times.Once());
        }

        [Fact]
        public async AsyncTask GetOperatorByIdAsync_WithInvalidId_PassesIdToService()
        {
            // Arrange
            var operatorID = -1;
            var error = OperatorError.NotFound(operatorID);

            _operatorServiceMock
                .Setup(x => x.GetOperatorByIdAsync(operatorID))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.GetOperatorByIdAsync(operatorID);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);

            _operatorServiceMock.Verify(
                x => x.GetOperatorByIdAsync(operatorID),
                Times.Once());
        }

        // =======================================================
        // CreateOperatorAsync
        // =======================================================

        [Fact]
        public async AsyncTask CreateOperatorAsync_WhenInputIsValid_ReturnsOk()
        {
            // Arrange
            var newOperator = new Operator
            {
                ID = 1,
                Username = "operator1",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();

            _operatorValidatorMock
                .Setup(x => x.ValidateAsync(
                    newOperator,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _operatorServiceMock
                .Setup(x => x.CreateOperatorAsync(newOperator))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.CreateOperatorAsync(newOperator);

            // Assert
            Assert.IsType<OkResult>(result);

            _operatorValidatorMock.Verify(
                x => x.ValidateAsync(
                    newOperator,
                    It.IsAny<CancellationToken>()),
                Times.Once());

            _operatorServiceMock.Verify(
                x => x.CreateOperatorAsync(newOperator),
                Times.Once());
        }

        [Fact]
        public async AsyncTask CreateOperatorAsync_WhenValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var newOperator = new Operator
            {
                ID = 1,
                Username = "operator1",
                Password = "Password123!"
            };

            var validationFailure = new ValidationFailure(
                "Name",
                "Operator name is required.");

            var validationResult = new ValidationResult(
                new[] { validationFailure });

            _operatorValidatorMock
                .Setup(x => x.ValidateAsync(
                    newOperator,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.CreateOperatorAsync(newOperator);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                validationResult.Errors,
                badRequestResult.Value);

            _operatorValidatorMock.Verify(
                x => x.ValidateAsync(
                    newOperator,
                    It.IsAny<CancellationToken>()),
                Times.Once());

            _operatorServiceMock.Verify(
                x => x.CreateOperatorAsync(It.IsAny<Operator>()),
                Times.Never());
        }

        [Fact]
        public async AsyncTask CreateOperatorAsync_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var newOperator = new Operator
            {
                ID = 1,
                Username = "operator1",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();

            var error = OperatorError.NotFound(newOperator.ID);

            _operatorValidatorMock
                .Setup(x => x.ValidateAsync(
                    newOperator,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _operatorServiceMock
                .Setup(x => x.CreateOperatorAsync(newOperator))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.CreateOperatorAsync(newOperator);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _operatorServiceMock.Verify(
                x => x.CreateOperatorAsync(newOperator),
                Times.Once());
        }

        // =======================================================
        // UpdateOperatorAsync
        // =======================================================

        [Fact]
        public async AsyncTask UpdateOperatorAsync_WhenInputIsValid_ReturnsOk()
        {
            // Arrange
            var operatorID = 1;

            var updatedOperator = new Operator
            {
                ID = operatorID,
                Username = "operator1",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();

            _operatorValidatorMock
                .Setup(x => x.ValidateAsync(
                    updatedOperator,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _operatorServiceMock
                .Setup(x => x.UpdateOperatorAsync(updatedOperator))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.UpdateOperatorAsync(
                operatorID,
                updatedOperator);

            // Assert
            Assert.IsType<OkResult>(result);

            _operatorValidatorMock.Verify(
                x => x.ValidateAsync(
                    updatedOperator,
                    It.IsAny<CancellationToken>()),
                Times.Once());

            _operatorServiceMock.Verify(
                x => x.UpdateOperatorAsync(updatedOperator),
                Times.Once());
        }

        [Fact]
        public async AsyncTask UpdateOperatorAsync_WhenIdsDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            var routeOperatorID = 1;

            var updatedOperator = new Operator
            {
                ID = 2,
                Username = "operator1",
                Password = "Password123!"
            };

            // Act
            var result = await _controller.UpdateOperatorAsync(
                routeOperatorID,
                updatedOperator);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                "Operator ID mismatch.",
                badRequestResult.Value);

            _operatorValidatorMock.Verify(
                x => x.ValidateAsync(
                    It.IsAny<Operator>(),
                    It.IsAny<CancellationToken>()),
                Times.Never());

            _operatorServiceMock.Verify(
                x => x.UpdateOperatorAsync(It.IsAny<Operator>()),
                Times.Never());
        }

        [Fact]
        public async AsyncTask UpdateOperatorAsync_WhenValidationFails_ReturnsBadRequest()
        {
            // Arrange
            var operatorID = 1;

            var updatedOperator = new Operator
            {
                ID = operatorID,
                Username = "operator1",
                Password = "Password123!"
            };

            var validationFailure = new ValidationFailure(
                "Name",
                "Operator name is required.");

            var validationResult = new ValidationResult(
                new[] { validationFailure });

            _operatorValidatorMock
                .Setup(x => x.ValidateAsync(
                    updatedOperator,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.UpdateOperatorAsync(
                operatorID,
                updatedOperator);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(
                validationResult.Errors,
                badRequestResult.Value);

            _operatorServiceMock.Verify(
                x => x.UpdateOperatorAsync(It.IsAny<Operator>()),
                Times.Never());
        }

        [Fact]
        public async AsyncTask UpdateOperatorAsync_WhenOperatorDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var operatorID = 999;

            var updatedOperator = new Operator
            {
                ID = operatorID,
                Username = "operator1",
                Password = "Password123!"
            };

            var validationResult = new ValidationResult();

            var error = OperatorError.NotFound(operatorID);

            _operatorValidatorMock
                .Setup(x => x.ValidateAsync(
                    updatedOperator,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _operatorServiceMock
                .Setup(x => x.UpdateOperatorAsync(updatedOperator))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.UpdateOperatorAsync(
                operatorID,
                updatedOperator);

            // Assert
            var notFoundResult =
                Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFoundResult.Value);

            _operatorServiceMock.Verify(
                x => x.UpdateOperatorAsync(updatedOperator),
                Times.Once());
        }

        // =======================================================
        // DeleteOperatorAsync
        // =======================================================

        [Fact]
        public async AsyncTask DeleteOperatorAsync_WhenOperatorExists_ReturnsOk()
        {
            // Arrange
            var operatorID = 1;

            _operatorServiceMock
                .Setup(x => x.DeleteOperatorAsync(operatorID))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.DeleteOperatorAsync(operatorID);

            // Assert
            Assert.IsType<OkResult>(result);

            _operatorServiceMock.Verify(
                x => x.DeleteOperatorAsync(operatorID),
                Times.Once());
        }

        [Fact]
        public async AsyncTask DeleteOperatorAsync_WhenOperatorDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var operatorID = 999;
            var error = OperatorError.NotFound(operatorID);

            _operatorServiceMock
                .Setup(x => x.DeleteOperatorAsync(operatorID))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.DeleteOperatorAsync(operatorID);

            // Assert
            var notFoundResult =
                Assert.IsType<NotFoundObjectResult>(result);

            Assert.Equal(error, notFoundResult.Value);

            _operatorServiceMock.Verify(
                x => x.DeleteOperatorAsync(operatorID),
                Times.Once());
        }

        [Fact]
        public async AsyncTask DeleteOperatorAsync_WithInvalidId_PassesIdToService()
        {
            // Arrange
            var operatorID = -1;
            var error = OperatorError.NotFound(operatorID);

            _operatorServiceMock
                .Setup(x => x.DeleteOperatorAsync(operatorID))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.DeleteOperatorAsync(operatorID);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);

            _operatorServiceMock.Verify(
                x => x.DeleteOperatorAsync(operatorID),
                Times.Once());
        }
    }
}
// ======================================================= { END OF FILE } ======================================================= //