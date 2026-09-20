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
        [HttpPost("{agentId}/spawn/powershell")]
        public async Task<IActionResult> QueueTask([FromRoute] int agentId, PowerShellDTO powershellDTO)
        {
            // Insert command into a queue
            AsyncCommandResponseDTO response = await this._commandService.QueueTask(powershellDTO, agentId);

            if (response.TaskID == null)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//