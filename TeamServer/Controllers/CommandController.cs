//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using TeamServer.Services;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Enums.Tasks;

namespace TeamServer.Controllers
{
    [Route(Routes.Commands)]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private CommandService _commandService;

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CommandController(CommandService commandService)
        {
            this._commandService = commandService;
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Queues a PowerShell task for the specified agent and returns the asynchronous command response.
        /// </summary>
        /// <remarks>Uses the command service to enqueue the task; the response contains the assigned
        /// TaskID when successful.</remarks>
        /// <param name="agentID">Target agent identifier.</param>
        /// <param name="powershellDTO">Payload describing the PowerShell command and execution options.</param>
        /// <returns>An IActionResult containing an AsyncCommandResponseDTO. Returns 200 OK with the response when the task is
        /// queued and 400 Bad Request if the task could not be created.</returns>
        [HttpPost("{agentID}/spawn/powershell")]
        public async Task<IActionResult> QueuePowerShellCommand([FromRoute] int agentID, PowerShellDTO powershellDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.POWERSHELL, TaskCode.NO_CODE, powershellDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueuePowerShellCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Execute a command using shell.
        /// </summary>
        /// <param name="agentID"></param>
        /// <param name="shellDTO"></param>
        /// <returns></returns>
        [HttpPost("{agentID}/spawn/shell")]
        public async Task<IActionResult> QueueShellCommand([FromRoute] int agentID, ShellDTO shellDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.SHELL, TaskCode.NO_CODE, shellDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueShellCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/spawn/runas")]
        public async Task<IActionResult> QueueRunAsCommand([FromRoute] int agentID, RunAsDTO runAsDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.RUN_AS, runAsDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueRunAsCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/spawn/run")]
        public async Task<IActionResult> QueueRunCommand([FromRoute] int agentID, RunDTO runDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.RUN, runDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueRunCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/spawn/runu")]
        public async Task<IActionResult> QueueRunUCommand([FromRoute] int agentID, RunUDTO runUDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.RUN_U, runUDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueRunUCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/killprocess")]
        public async Task<IActionResult> QueueKillCommand([FromRoute] int agentID, KillProcessDTO killProcessDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.KILL_PROCESS, killProcessDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueKillCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/escalate")]
        public async Task<IActionResult> QueueEscalateCommand([FromRoute] int agentID, EscalateDTO escalateDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.NO_CODE, escalateDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueEscalateCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/dotnetassembly")]
        public async Task<IActionResult> QueueDotNetAsmCommand([FromRoute] int agentID, DotNetAsmDTO dotNetAsmDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.NET_ASM, TaskCode.NO_CODE, dotNetAsmDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueDotNetAsmCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/bof")]
        public async Task<IActionResult> QueueBofCommand([FromRoute] int agentID, BofDTO bofDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.OBJECT_FILE, TaskCode.NO_CODE, bofDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueBofCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/filedownload")]
        public async Task<IActionResult> QueueFileDownloadCommand([FromRoute] int agentID, DownloadDTO downloadDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.DOWNLOAD, TaskCode.DOWNLOAD, downloadDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueFileDownloadCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/cancelfiledownload")]
        public async Task<IActionResult> QueueCancelFileDownloadCommand([FromRoute] int agentID, CancelDownloadDTO cancelDownloadDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.NO_CODE, cancelDownloadDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueCancelFileDownloadCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/upload")]
        public async Task<IActionResult> QueueUploadCommand([FromRoute] int agentID, UploadDTO uploadDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.UPLOAD, TaskCode.NO_CODE, uploadDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueUploadCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/set/spawnto")]
        public async Task<IActionResult> QueueSpawnToCommand([FromRoute] int agentID, SpawnToDTO spawnToDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.SPAWN_TO, spawnToDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueSpawnToCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/set/updatehosts")]
        public async Task<IActionResult> QueueUpdateHostsCommand([FromRoute] int agentID, UpdateHostsDTO updateHostsDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.SET, TaskCode.UPDATE_HOSTS, updateHostsDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueUpdateHostsCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/set/sleep")]
        public async Task<IActionResult> QueueSetSleepCommand([FromRoute] int agentID, SetSleepDTO setSleepDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.PROCESS, TaskCode.SLEEP, setSleepDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueSetSleepCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/kill")]
        public async Task<IActionResult> QueueKillAgentCommand([FromRoute] int agentID, KillAgentDTO killAgentDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.KILL, TaskCode.NO_CODE, killAgentDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueKillAgentCommand), response.Response) : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentID}/execute/getuid")]
        public async Task<IActionResult> QueueGetUIDCommand([FromRoute] int agentID, GetUIDDTO getUIDDTO)
        {
            var response = await this._commandService.QueueTaskAsync(agentID, TaskType.GET_UID, TaskCode.NO_CODE, getUIDDTO);
            return response.IsSuccess ? CreatedAtAction(nameof(QueueGetUIDCommand), response.Response) : BadRequest(response.Error);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//