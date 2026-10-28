// ============================================================ { START OF FILE } ============================================================ //
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.DTOs.Task;
using Xunit;

using AsyncTask = System.Threading.Tasks.Task;

namespace TeamServer.Tests.Controllers
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _taskServiceMock;
        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            _taskServiceMock = new Mock<ITaskService>();
            _controller = new TaskController(_taskServiceMock.Object);
        }

        [Fact]
        public async AsyncTask GetTasks_WhenSuccessful_ReturnsOkWithTasks()
        {
            var tasks = new List<TaskResponse>();

            _taskServiceMock
                .Setup(x => x.GetAllAgentTasks())
                .ReturnsAsync(tasks);

            var result = await _controller.GetTasks();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(tasks, okResult.Value);

            _taskServiceMock.Verify(
                x => x.GetAllAgentTasks(),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetTaskDetailsByID_WhenSuccessful_ReturnsOkWithTask()
        {
            int taskID = 42;

            var task = new GetTaskDetailsResponse(
                42,
                10,
                "TestCommand",
                JsonDocument.Parse("{}"),
                DateTime.UtcNow);

            _taskServiceMock
                .Setup(x => x.GetTaskDetailsByIDAsync(taskID))
                .ReturnsAsync(task);

            var result = await _controller.GetTaskDetailsByID(taskID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(task, okResult.Value);

            _taskServiceMock.Verify(
                x => x.GetTaskDetailsByIDAsync(taskID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetTaskDetailsByID_WhenNotFound_ReturnsNotFoundWithError()
        {
            int taskID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Task",
                taskID.ToString());

            _taskServiceMock
                .Setup(x => x.GetTaskDetailsByIDAsync(taskID))
                .ReturnsAsync(error);

            var result = await _controller.GetTaskDetailsByID(taskID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _taskServiceMock.Verify(
                x => x.GetTaskDetailsByIDAsync(taskID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetTasksByAgentID_WhenSuccessful_ReturnsOkWithTasks()
        {
            int agentID = 10;

            var tasks = new List<TaskResponse>();

            _taskServiceMock
                .Setup(x => x.GetQueuedTasksByAgentID(agentID))
                .ReturnsAsync(tasks);

            var result = await _controller.GetTasksByAgentID(agentID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(tasks, okResult.Value);

            _taskServiceMock.Verify(
                x => x.GetQueuedTasksByAgentID(agentID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetTasksByAgentID_WhenNotFound_ReturnsNotFoundWithError()
        {
            int agentID = 10;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Agent",
                agentID.ToString());

            _taskServiceMock
                .Setup(x => x.GetQueuedTasksByAgentID(agentID))
                .ReturnsAsync(error);

            var result = await _controller.GetTasksByAgentID(agentID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _taskServiceMock.Verify(
                x => x.GetQueuedTasksByAgentID(agentID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask GetActiveDownloadsByAgentID_ReturnsOk()
        {
            var result = await _controller.GetActiveDownloadsByAgentID(10);

            Assert.IsType<OkResult>(result);

            _taskServiceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async AsyncTask StopTask_WhenSuccessful_ReturnsOk()
        {
            int taskID = 42;

            _taskServiceMock
                .Setup(x => x.RemoveTaskByIDAsync(taskID))
                .ReturnsAsync(Trinity.Shared.Results.Result.Success());

            var result = await _controller.StopTask(taskID);

            Assert.IsType<OkResult>(result);

            _taskServiceMock.Verify(
                x => x.RemoveTaskByIDAsync(taskID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask StopTask_WhenNotFound_ReturnsNotFoundWithError()
        {
            int taskID = 42;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Task",
                taskID.ToString());

            _taskServiceMock
                .Setup(x => x.RemoveTaskByIDAsync(taskID))
                .ReturnsAsync(error);

            var result = await _controller.StopTask(taskID);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Same(error, notFoundResult.Value);

            _taskServiceMock.Verify(
                x => x.RemoveTaskByIDAsync(taskID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask ClearQueue_WhenSuccessful_ReturnsOk()
        {
            int agentID = 10;

            _taskServiceMock
                .Setup(x => x.ClearAgentTasksAsync(agentID))
                .ReturnsAsync(Trinity.Shared.Results.Result.Success());

            var result = await _controller.ClearQueue(agentID);

            Assert.IsType<OkResult>(result);

            _taskServiceMock.Verify(
                x => x.ClearAgentTasksAsync(agentID),
                Times.Once);
        }

        [Fact]
        public async AsyncTask ClearQueue_WhenNotFound_ReturnsBadRequestWithError()
        {
            int agentID = 10;

            var error = Trinity.Shared.Errors.Error.NotFound(
                "Agent",
                agentID.ToString());

            _taskServiceMock
                .Setup(x => x.ClearAgentTasksAsync(agentID))
                .ReturnsAsync(error);

            var result = await _controller.ClearQueue(agentID);

            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Same(error, badRequestResult.Value);

            _taskServiceMock.Verify(
                x => x.ClearAgentTasksAsync(agentID),
                Times.Once);
        }
    }
}
// ============================================================ { END OF FILE } ============================================================ //