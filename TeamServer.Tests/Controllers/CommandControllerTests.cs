// ======================================================= { START OF FILE } ======================================================= //
using Microsoft.AspNetCore.Mvc;
using Moq;
using TeamServer.Controllers;
using TeamServer.Interface;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Enums.Tasks;
using Trinity.Shared.Errors;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Results;
using AsyncTask = System.Threading.Tasks.Task;

namespace TeamServer.Tests.Controllers
{
    public class CommandControllerTests
    {
        private readonly Mock<ICommandService> _commandServiceMock;
        private readonly CommandController _controller;

        public CommandControllerTests()
        {
            _commandServiceMock = new Mock<ICommandService>();

            _controller = new CommandController(
                _commandServiceMock.Object);
        }

        // =======================================================
        // QueuePowerShellCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueuePowerShellCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new PowerShellDTO
            {
                Commandlet = "whoami",
                Arguements = ""
            };

            var response = new CommandCreationResponse(
                "Tasked agent to run: whoami",
                100);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.POWERSHELL,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueuePowerShellCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.POWERSHELL,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueuePowerShellCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new PowerShellDTO
            {
                Commandlet = "whoami",
                Arguements = ""
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.POWERSHELL,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueuePowerShellCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.POWERSHELL,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueShellCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueShellCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new ShellDTO
            {
                Command = "whoami"
            };

            var response = new CommandCreationResponse(
                "Tasked agent to run: whoami",
                101);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SHELL,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueShellCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SHELL,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueShellCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new ShellDTO
            {
                Command = "whoami"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SHELL,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueShellCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SHELL,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueRunAsCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueRunAsCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new RunAsDTO
            {
                DomainUser = "DOMAIN\\user",
                Password = "Password123!",
                Command = "cmd.exe",
                Arguements = "/c whoami"
            };

            var response = new CommandCreationResponse(
                "Tasked agent to run command as DOMAIN\\user",
                102);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_AS,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueRunAsCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_AS,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueRunAsCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new RunAsDTO
            {
                DomainUser = "DOMAIN\\user",
                Password = "Password123!",
                Command = "cmd.exe",
                Arguements = "/c whoami"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_AS,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueRunAsCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_AS,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueRunCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueRunCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new RunDTO
            {
                Program = "notepad.exe",
                Arguements = ""
            };

            var response = new CommandCreationResponse(
                "Tasked agent to run program: notepad.exe",
                103);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueRunCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueRunCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new RunDTO
            {
                Program = "notepad.exe",
                Arguements = ""
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueRunCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueRunUCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueRunUCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new RunUDTO
            {
                PID = 1234,
                Command = "cmd.exe",
                Arguements = "/c whoami"
            };

            var response = new CommandCreationResponse(
                "Tasked agent to run command under process: 1234",
                104);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_U,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueRunUCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_U,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueRunUCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new RunUDTO
            {
                PID = 1234,
                Command = "cmd.exe",
                Arguements = "/c whoami"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_U,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueRunUCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.RUN_U,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueKillCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueKillCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new KillProcessDTO
            {
                PID = 1234
            };

            var response = new CommandCreationResponse(
                "Tasked agent to kill process: 1234",
                105);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.KILL_PROCESS,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueKillCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.KILL_PROCESS,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueKillCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new KillProcessDTO
            {
                PID = 1234
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.KILL_PROCESS,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueKillCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.KILL_PROCESS,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueEscalateCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueEscalateCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new EscalateDTO
            {
                RegKey = "HKLM\\Software\\Test",
                TargetBinary = "test.exe"
            };

            var response = new CommandCreationResponse(
                "Escalation queued",
                106);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueEscalateCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueEscalateCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new EscalateDTO
            {
                RegKey = "HKLM\\Software\\Test",
                TargetBinary = "test.exe"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueEscalateCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueDotNetAsmCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueDotNetAsmCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new DotNetAsmDTO
            {
                AssemblyPath = "test.dll",
                Arguments = ""
            };

            var response = new CommandCreationResponse(
                "Assembly queued",
                107);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.NET_ASM,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueDotNetAsmCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.NET_ASM,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueDotNetAsmCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new DotNetAsmDTO
            {
                AssemblyPath = "test.dll",
                Arguments = ""
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.NET_ASM,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueDotNetAsmCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.NET_ASM,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueBofCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueBofCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new BofDTO
            {
                BofPath = "test.o",
                Entrypoint = "go",
                Arguements = ""
            };

            var response = new CommandCreationResponse(
                "BOF queued",
                108);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.OBJECT_FILE,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueBofCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.OBJECT_FILE,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueBofCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new BofDTO
            {
                BofPath = "test.o",
                Entrypoint = "go",
                Arguements = ""
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.OBJECT_FILE,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueBofCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.OBJECT_FILE,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueFileDownloadCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueFileDownloadCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new DownloadDTO
            {
                Path = "C:\\Temp\\test.txt"
            };

            var response = new CommandCreationResponse(
                "Tasked agent to download C:\\Temp\\test.txt",
                109);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.DOWNLOAD,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueFileDownloadCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.DOWNLOAD,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueFileDownloadCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new DownloadDTO
            {
                Path = "C:\\Temp\\test.txt"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.DOWNLOAD,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueFileDownloadCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.DOWNLOAD,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueCancelFileDownloadCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueCancelFileDownloadCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new CancelDownloadDTO
            {
                TaskID = 109
            };

            var response = new CommandCreationResponse(
                "Tasked agent to cancel download with taskID: 109",
                110);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);
            // Act
            var result = await _controller.QueueCancelFileDownloadCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueCancelFileDownloadCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new CancelDownloadDTO
            {
                TaskID = 109
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueCancelFileDownloadCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueUploadCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueUploadCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new UploadDTO
            {
                Path = "C:\\Temp\\upload.txt"
            };

            var response = new CommandCreationResponse(
                "Upload queued",
                111);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.CANCEL_DOWNLOAD,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueUploadCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.CANCEL_DOWNLOAD,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueUploadCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new UploadDTO
            {
                Path = "C:\\Temp\\upload.txt"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.CANCEL_DOWNLOAD,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueUploadCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.DOWNLOAD,
                    TaskCode.CANCEL_DOWNLOAD,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueSpawnToCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueSpawnToCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new SpawnToDTO
            {
                Path = "C:\\Windows\\System32\\notepad.exe"
            };

            var response = new CommandCreationResponse(
                "Tasked agent to set spawn to: C:\\Windows\\System32\\notepad.exe",
                112);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SPAWN_TO,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueSpawnToCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SPAWN_TO,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueSpawnToCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new SpawnToDTO
            {
                Path = "C:\\Windows\\System32\\notepad.exe"
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SPAWN_TO,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueSpawnToCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SPAWN_TO,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueUpdateHostsCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueUpdateHostsCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new UpdateHostsDTO
            {
                Hosts = new List<string>
                {
                    "127.0.0.1 localhost",
                    "192.168.1.10 server"
                }
            };

            var response = new CommandCreationResponse(
                "Tasked agent to update hosts: System.Collections.Generic.List`1[System.String]",
                113);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SET,
                    TaskCode.UPDATE_HOSTS,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueUpdateHostsCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SET,
                    TaskCode.UPDATE_HOSTS,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueUpdateHostsCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new UpdateHostsDTO
            {
                Hosts = new List<string>
                {
                    "127.0.0.1 localhost"
                }
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SET,
                    TaskCode.UPDATE_HOSTS,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueUpdateHostsCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.SET,
                    TaskCode.UPDATE_HOSTS,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueSetSleepCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueSetSleepCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new SetSleepDTO
            {
                Sleep = 30,
                Jitter = 10
            };

            var response = new CommandCreationResponse(
                "Tasked agent to sleep for 30s",
                114);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SLEEP,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueSetSleepCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SLEEP,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueSetSleepCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new SetSleepDTO
            {
                Sleep = 30,
                Jitter = 10
            };

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SLEEP,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueSetSleepCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.PROCESS,
                    TaskCode.SLEEP,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueKillAgentCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueKillAgentCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new KillAgentDTO();

            var response = new CommandCreationResponse(
                "Tasked agent to terminate",
                115);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.KILL,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueKillAgentCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.KILL,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueKillAgentCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new KillAgentDTO();

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.KILL,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueKillAgentCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.KILL,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        // =======================================================
        // QueueGetUIDCommand
        // =======================================================

        [Fact]
        public async AsyncTask QueueGetUIDCommand_WhenServiceSucceeds_ReturnsCreated()
        {
            // Arrange
            var agentID = 1;

            var dto = new GetUIDDTO();

            var response = new CommandCreationResponse(
                "Tasked agent to get user ID",
                116);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.GET_UID,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.QueueGetUIDCommand(
                agentID,
                dto);

            // Assert
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result);

            Assert.Equal(response, createdResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.GET_UID,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }

        [Fact]
        public async AsyncTask QueueGetUIDCommand_WhenServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var agentID = 1;

            var dto = new GetUIDDTO();

            var error = AgentError.NotFound(agentID);

            _commandServiceMock
                .Setup(x => x.QueueTaskAsync(
                    agentID,
                    TaskType.GET_UID,
                    TaskCode.NO_CODE,
                    dto))
                .ReturnsAsync(error);

            // Act
            var result = await _controller.QueueGetUIDCommand(
                agentID,
                dto);

            // Assert
            var badRequestResult =
                Assert.IsType<BadRequestObjectResult>(result);

            Assert.Equal(error, badRequestResult.Value);

            _commandServiceMock.Verify(
                x => x.QueueTaskAsync(
                    agentID,
                    TaskType.GET_UID,
                    TaskCode.NO_CODE,
                    dto),
                Times.Once());
        }
    }
}
// ======================================================= { END OF FILE } ======================================================= //