// ======================================================= { START OF FILE } ======================================================= //
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.Enums;
using Trinity.Shared.Models;
using Trinity.Shared.Results;
using AsyncTask = System.Threading.Tasks.Task;

namespace TeamServer.Tests.Controllers
{
    public class CampaignControllerTests
    {
        private readonly Mock<ICampaignService> _campaignServiceMock;
        private readonly Mock<IValidator<Campaign>> _campaignValidatorMock;
        private readonly CampaignController _controller;

        public CampaignControllerTests()
        {
            _campaignServiceMock = new Mock<ICampaignService>();
            _campaignValidatorMock = new Mock<IValidator<Campaign>>();

            _controller = new CampaignController(
                _campaignServiceMock.Object,
                _campaignValidatorMock.Object);
        }

        // =======================================================
        // GetCampaignsAsync
        // =======================================================

        [Fact]
        public async AsyncTask GetCampaignsAsync_ReturnsOk_WhenSuccessful()
        {
            var campaigns = new List<Campaign>
            {
                new Campaign { ID = 1, Name = "Campaign One" },
                new Campaign { ID = 2, Name = "Campaign Two" }
            };

            _campaignServiceMock
                .Setup(x => x.GetCampaignsAsync())
                .ReturnsAsync(campaigns);

            var result = await _controller.GetCampaignsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(campaigns, okResult.Value);

            _campaignServiceMock.Verify(
                x => x.GetCampaignsAsync(),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetCampaignsAsync_ReturnsBadRequest_WhenServiceFails()
        {
            var error = new Trinity.Shared.Errors.Error(
                "TEST_ERROR",
                "Unable to retrieve campaigns",
                ErrorType.Failure);

            _campaignServiceMock
                .Setup(x => x.GetCampaignsAsync())
                .ReturnsAsync(error);

            var result = await _controller.GetCampaignsAsync();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Same(error, badRequestResult.Value);

            _campaignServiceMock.Verify(
                x => x.GetCampaignsAsync(),
                Times.Once);
        }

        // =======================================================
        // GetCampaignByIdAsync
        // =======================================================

        [Fact]
        public async AsyncTask GetCampaignByIdAsync_ReturnsOk_WhenSuccessful()
        {
            int campaignID = 10;

            var campaign = new Campaign
            {
                ID = campaignID,
                Name = "Test Campaign"
            };

            _campaignServiceMock
                .Setup(x => x.GetCampaignByIdAsync(campaignID))
                .ReturnsAsync(campaign);

            var result = await _controller.GetCampaignByIdAsync(campaignID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(campaign, okResult.Value);

            _campaignServiceMock.Verify(
                x => x.GetCampaignByIdAsync(campaignID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetCampaignByIdAsync_ReturnsNotFound_WhenServiceFails()
        {
            int campaignID = 10;

            var error = new Trinity.Shared.Errors.Error(
                "NOT_FOUND",
                "Campaign not found",
                ErrorType.NotFound);

            _campaignServiceMock
                .Setup(x => x.GetCampaignByIdAsync(campaignID))
                .ReturnsAsync(error);

            var result = await _controller.GetCampaignByIdAsync(campaignID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _campaignServiceMock.Verify(
                x => x.GetCampaignByIdAsync(campaignID),
                Times.Once);
        }

        // =======================================================
        // CreateCampaignAsync
        // =======================================================

        [Fact]
        public async AsyncTask CreateCampaignAsync_ReturnsOk_WhenValidAndSuccessful()
        {
            var campaign = new Campaign
            {
                ID = 1,
                Name = "Test Campaign"
            };

            _campaignValidatorMock
                .Setup(x => x.ValidateAsync(
                    campaign,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _campaignServiceMock
                .Setup(x => x.CreateCampaignAsync(campaign))
                .ReturnsAsync(Result.Success());

            var result = await _controller.CreateCampaignAsync(campaign);

            Assert.IsType<OkResult>(result);

            _campaignValidatorMock.Verify(
                x => x.ValidateAsync(
                    campaign,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _campaignServiceMock.Verify(
                x => x.CreateCampaignAsync(campaign),
                Times.Once);
        }

        [Fact]
        public async AsyncTask CreateCampaignAsync_ReturnsBadRequest_WhenValidationFails()
        {
            var campaign = new Campaign
            {
                ID = 1,
                Name = ""
            };

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required")
            };

            _campaignValidatorMock
                .Setup(x => x.ValidateAsync(
                    campaign,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationErrors));

            var result = await _controller.CreateCampaignAsync(campaign);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(badRequestResult.Value);

            Assert.Single(errors);
            Assert.Equal("Name", errors.First().PropertyName);
            Assert.Equal("Name is required", errors.First().ErrorMessage);

            _campaignValidatorMock.Verify(
                x => x.ValidateAsync(
                    campaign,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _campaignServiceMock.Verify(
                x => x.CreateCampaignAsync(It.IsAny<Campaign>()),
                Times.Never);
        }

        // =======================================================
        // UpdateCampaignAsync
        // =======================================================

        [Fact]
        public async AsyncTask UpdateCampaignAsync_ReturnsOk_WhenValidAndSuccessful()
        {
            int campaignID = 25;

            var campaign = new Campaign
            {
                ID = 999,
                Name = "Updated Campaign"
            };

            _campaignValidatorMock
                .Setup(x => x.ValidateAsync(
                    It.IsAny<Campaign>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _campaignServiceMock
                .Setup(x => x.UpdateCampaignAsync(It.IsAny<Campaign>()))
                .ReturnsAsync(Result.Success());

            var result = await _controller.UpdateCampaignAsync(
                campaignID,
                campaign);

            Assert.IsType<OkResult>(result);

            Assert.Equal(campaignID, campaign.ID);

            _campaignValidatorMock.Verify(
                x => x.ValidateAsync(
                    campaign,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _campaignServiceMock.Verify(
                x => x.UpdateCampaignAsync(campaign),
                Times.Once);
        }

        [Fact]
        public async AsyncTask UpdateCampaignAsync_ReturnsBadRequest_WhenValidationFails()
        {
            int campaignID = 25;

            var campaign = new Campaign
            {
                ID = 999,
                Name = ""
            };

            var validationErrors = new List<ValidationFailure>
            {
                new ValidationFailure("Name", "Name is required")
            };

            _campaignValidatorMock
                .Setup(x => x.ValidateAsync(
                    It.IsAny<Campaign>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationErrors));

            var result = await _controller.UpdateCampaignAsync(
                campaignID,
                campaign);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(badRequestResult.Value);

            Assert.Single(errors);
            Assert.Equal("Name", errors.First().PropertyName);
            Assert.Equal("Name is required", errors.First().ErrorMessage);

            Assert.Equal(campaignID, campaign.ID);

            _campaignValidatorMock.Verify(
                x => x.ValidateAsync(
                    campaign,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _campaignServiceMock.Verify(
                x => x.UpdateCampaignAsync(It.IsAny<Campaign>()),
                Times.Never);
        }

        // =======================================================
        // DeleteCampaignAsync
        // =======================================================

        [Fact]
        public async AsyncTask DeleteCampaignAsync_ReturnsOk_WhenSuccessful()
        {
            int campaignID = 15;

            _campaignServiceMock
                .Setup(x => x.DeleteCampaignAsync(campaignID))
                .ReturnsAsync(Result.Success());

            var result = await _controller.DeleteCampaignAsync(campaignID);

            Assert.IsType<OkResult>(result);

            _campaignServiceMock.Verify(
                x => x.DeleteCampaignAsync(campaignID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask DeleteCampaignAsync_ReturnsNotFound_WhenServiceFails()
        {
            int campaignID = 15;

            var error = new Trinity.Shared.Errors.Error(
                "NOT_FOUND",
                "Campaign not found"
                , ErrorType.NotFound);

            _campaignServiceMock
                .Setup(x => x.DeleteCampaignAsync(campaignID))
                .ReturnsAsync(error);

            var result = await _controller.DeleteCampaignAsync(campaignID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _campaignServiceMock.Verify(
                x => x.DeleteCampaignAsync(campaignID),
                Times.Once);
        }

        // =======================================================
        // GetOperatorsByCampaignIdAsync
        // =======================================================

        [Fact]
        public async AsyncTask GetOperatorsByCampaignIdAsync_ReturnsOk_WhenSuccessful()
        {
            int campaignID = 5;

            var operators = new List<Operator>();

            _campaignServiceMock
                .Setup(x => x.GetOperatorsByCampaignIdAsync(campaignID))
                .ReturnsAsync(operators);

            var result = await _controller.GetOperatorsByCampaignIdAsync(campaignID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(operators, okResult.Value);

            _campaignServiceMock.Verify(
                x => x.GetOperatorsByCampaignIdAsync(campaignID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetOperatorsByCampaignIdAsync_ReturnsNotFound_WhenServiceFails()
        {
            int campaignID = 5;

            var error = new Trinity.Shared.Errors.Error(
                "NOT_FOUND",
                "Campaign not found",
                ErrorType.NotFound);

            _campaignServiceMock
                .Setup(x => x.GetOperatorsByCampaignIdAsync(campaignID))
                .ReturnsAsync(error);

            var result = await _controller.GetOperatorsByCampaignIdAsync(campaignID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _campaignServiceMock.Verify(
                x => x.GetOperatorsByCampaignIdAsync(campaignID),
                Times.Once);
        }

        // =======================================================
        // AssignOperatorToCampaignAsync
        // =======================================================

        [Fact]
        public async AsyncTask AssignOperatorToCampaignAsync_ReturnsOk_WhenSuccessful()
        {
            int operatorID = 10;
            int campaignID = 20;

            _campaignServiceMock
                .Setup(x => x.AssignOperatorToCampaignAsync(
                    operatorID,
                    campaignID))
                .ReturnsAsync(Result.Success());

            var result = await _controller.AssignOperatorToCampaignAsync(
                operatorID,
                campaignID);

            Assert.IsType<OkResult>(result);

            _campaignServiceMock.Verify(
                x => x.AssignOperatorToCampaignAsync(
                    operatorID,
                    campaignID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask AssignOperatorToCampaignAsync_ReturnsNotFound_WhenServiceFails()
        {
            int operatorID = 10;
            int campaignID = 20;

            var error = new Trinity.Shared.Errors.Error(
                "NOT_FOUND",
                "Campaign not found",
                ErrorType.NotFound);

            _campaignServiceMock
                .Setup(x => x.AssignOperatorToCampaignAsync(
                    operatorID,
                    campaignID))
                .ReturnsAsync(error);

            var result = await _controller.AssignOperatorToCampaignAsync(
                operatorID,
                campaignID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _campaignServiceMock.Verify(
                x => x.AssignOperatorToCampaignAsync(
                    operatorID,
                    campaignID),
                Times.Once);
        }

        // =======================================================
        // UnassignOperatorFromCampaignAsync
        // =======================================================

        [Fact]
        public async AsyncTask UnassignOperatorFromCampaignAsync_ReturnsOk_WhenSuccessful()
        {
            int operatorID = 10;
            int campaignID = 20;

            _campaignServiceMock
                .Setup(x => x.RemoveOperatorFromCampaignAsync(
                    operatorID,
                    campaignID))
                .ReturnsAsync(Result.Success());

            var result = await _controller.UnassignOperatorFromCampaignAsync(
                operatorID,
                campaignID);

            Assert.IsType<OkResult>(result);

            _campaignServiceMock.Verify(
                x => x.RemoveOperatorFromCampaignAsync(
                    operatorID,
                    campaignID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask UnassignOperatorFromCampaignAsync_ReturnsNotFound_WhenServiceFails()
        {
            int operatorID = 10;
            int campaignID = 20;

            var error = new Trinity.Shared.Errors.Error(
                "NOT_FOUND",
                "Campaign not found",
                ErrorType.NotFound);

            _campaignServiceMock
                .Setup(x => x.RemoveOperatorFromCampaignAsync(
                    operatorID,
                    campaignID))
                .ReturnsAsync(error);

            var result = await _controller.UnassignOperatorFromCampaignAsync(
                operatorID,
                campaignID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _campaignServiceMock.Verify(
                x => x.RemoveOperatorFromCampaignAsync(
                    operatorID,
                    campaignID),
                Times.Once);
        }
    }
}
// ======================================================= { END OF FILE } ======================================================= //